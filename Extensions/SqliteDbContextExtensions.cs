using Discord;
using KushBot.Data;
using KushBot.DataClasses;
using KushBot.Global;
using KushBot.Resources.Database;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace KushBot;

// Move to a repository and repository provider pattern
// Make a server provider and a middleware to set value for the provider, then use provider to provide the correct dbContext connection
public static class SqliteDbContextExtensions
{
    public static async Task<KushBotUser> GetKushBotUserAsync(this SqliteDbContext dbContext, ulong userId, UserDtoFeatures features = UserDtoFeatures.None)
    {
        var query = dbContext.Users
            .Where(e => e.Id == userId);

        if (features.HasFlag(UserDtoFeatures.Items))
        {
            query = query
            .Include(e => e.Items)
                    .ThenInclude(e => e.ItemStats);
        }

        var includes = new Dictionary<UserDtoFeatures, Expression<Func<KushBotUser, object>>>
        {
            { UserDtoFeatures.Buffs, e => e.UserBuffs },
            { UserDtoFeatures.Pictures, e => e.UserPictures },
            { UserDtoFeatures.Infections, e => e.UserInfections },
            { UserDtoFeatures.Plots, e => e.UserPlots }
        };

        foreach (var include in includes)
        {
            if (features.HasFlag(include.Key))
            {
                query = query.Include(include.Value);
            }
        }

        var user = await query.FirstOrDefaultAsync();

        if(features.HasFlag(UserDtoFeatures.Plots))
        {
            var factory = new PlotFactory();
            user.UserPlots = user.UserPlots.Select(e => factory.CreatePlot(e)).ToList();
        }

        if (features.HasFlag(UserDtoFeatures.Quests))
        {
            user.UserQuests = new UserQuests(dbContext.Quests.Where(e => e.UserId == user.Id).Include(e => e.Requirements.OrderBy(e => e.Type)).ToList());

            var relevantLogTypes = user.UserQuests.Where(e => !e.IsCompleted).SelectMany(e => e.GetRelevantEventTypes()).Distinct();

            user.UserEvents = new UserEvents(dbContext.UserEvents.Where(e => e.UserId == user.Id
                                                    && e.CreationTime > TimeHelper.LastMidnight
                                                    && relevantLogTypes.Contains(e.Type))
                                            .AsNoTracking()
                                            .OrderBy(e => e.CreationTime)
                                            .ToList());

            if (user.UserQuests.Any(e => !e.IsCompleted) && user.Items == null)
            {
                user.Items = GetUserItemsInternal(dbContext, userId, true);
            }

            if (user.UserQuests.Any(e => !e.IsCompleted) && user.Pets == null)
            {
                user.Pets = await GetUserPetsInternal(dbContext, userId, user.Items);
            }
        }

        if (features.HasFlag(UserDtoFeatures.Pets))
        {
            user.Pets = await GetUserPetsInternal(dbContext, userId, user.Items);
        }

        return user;
    }

    private static async Task<UserPets> GetUserPetsInternal(SqliteDbContext dbContext, ulong userId, UserItems items = null)
    {
        var pets = new UserPets(await dbContext.UserPets.Where(e => e.UserId == userId).ToListAsync());
        var equippedItems = items ?? GetUserItemsInternal(dbContext, userId, true);

        foreach (var itemStat in equippedItems.SelectMany(e => e.ItemStats))
        {
            if (itemStat.PetType != null && pets.ContainsKey(itemStat.PetType.Value))
            {
                pets[itemStat.PetType.Value].ItemLevel += (int)itemStat.Bonus;
            }
        }

        return pets;
    }

    private static UserItems GetUserItemsInternal(SqliteDbContext dbContext, ulong userId, bool? isEquipped = null)
    {
        return new UserItems(dbContext.Items
            .Include(e => e.ItemStats)
            .Where(e => e.OwnerId == userId && (!isEquipped.HasValue || e.IsEquipped == isEquipped.Value))
            .ToList()) ?? new();
    }

    public static async Task<UserPets> GetUserPetsAsync(this SqliteDbContext dbContext, ulong userId)
    {
        return await GetUserPetsInternal(dbContext, userId);
    }
}
