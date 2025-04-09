using ShadowRealm.Api.Models;
namespace ShadowRealm.Api.Data;

public static class DbSeeder
{
    public static void SeedInitialData(AppDbContext db)
    {
        if (!db.Quests.Any())
        {
            db.Quests.AddRange( new List<Quest>
            {
                new ()
                {
                    Title = "Najdi ztracený amulet", 
                    Description = "podle pověstí by se měl amulet nacházet v jeskyni", 
                    RevardXP = 100
                },
                new ()
                {
                    Title = "Defeat the Goblin King", 
                    Description = "Defeat the Goblin King in the forest", 
                    RevardXP = 200
                },
                new ()
                {
                    Title = "Rescue the Princess", 
                    Description = "Rescue the princess from the dragon", 
                    RevardXP = 300
                },
                new ()
                {
                    Title = "Find the Lost Treasure", 
                    Description = "Find the lost treasure in the mountains", 
                    RevardXP = 400
                },
            });
            
            db.SaveChanges();
        }
    }
}