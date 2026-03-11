using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using Plugin.Core.Enums;
using Plugin.Core;
using Plugin.Core.SQL;

public class SeasonPass
{
    public static void LoadSeasonPass()
    {

        using (NpgsqlConnection connection = ConnectionSQL.GetInstance().Conn())
        {
            try
            { 
                NpgsqlCommand Command = connection.CreateCommand();
                connection.Open();

                List<SeasonInfo> seasonInfos = LoadSeasonInfo(connection);
                List<SeasonCard> seasonCards = LoadSeasonCards(connection);
                List<DailyCard> dailyCards = LoadDailyCards(connection);
                List<SeasonDate> seasonDates = LoadSeasonDates(connection);
                CLogger.Print("Load All SeasonPass info successfully.", LoggerType.Debug);
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error: {ex.Message}", LoggerType.Error);
            }
        }
    }

    static List<SeasonInfo> LoadSeasonInfo(NpgsqlConnection connection)
    {
        List<SeasonInfo> seasons = new List<SeasonInfo>();
        string query = "SELECT * FROM SeasonInfo;";

        using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
        using (NpgsqlDataReader reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                seasons.Add(new SeasonInfo
                {
                    SeasonID = reader.GetInt32(0),
                    SeasonName = reader.GetString(1),
                    SeasonDays = reader.GetInt16(2), // SMALLINT in PostgreSQL
                    StartDate = reader.GetInt64(3), // BIGINT
                    EndDate = reader.GetInt64(4),
                    IsActive = reader.GetBoolean(5),
                    CurrentLevel = reader.GetInt16(6),
                    TotalEarnedPoints = reader.GetInt32(7),
                    NormalLevelsComplete = reader.GetInt16(8),
                    PremiumLevelsComplete = reader.GetInt16(9),
                    EnablePremium = reader.GetBoolean(10),
                    CurrentBuyLevel = reader.GetInt16(11),
                    SeasonStatus = reader.GetInt16(12)
                });
            }
        }
        return seasons;
    }

    static List<SeasonCard> LoadSeasonCards(NpgsqlConnection connection)
    {
        List<SeasonCard> cards = new List<SeasonCard>();
        string query = "SELECT * FROM SeasonCards;";

        using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
        using (NpgsqlDataReader reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                cards.Add(new SeasonCard
                {
                    CardID = reader.GetInt32(0),
                    SeasonID = reader.GetInt32(1),
                    NormalCardID = reader.GetInt64(2),
                    PremiumCardBID = reader.GetInt64(3),
                    PremiumCardAID = reader.GetInt64(4),
                    TotalPointsRequired = reader.GetInt32(5)
                });
            }
        }
        return cards;
    }

    static List<DailyCard> LoadDailyCards(NpgsqlConnection connection)
    {
        List<DailyCard> dailyCards = new List<DailyCard>();
        string query = "SELECT * FROM DailyCards;";

        using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
        using (NpgsqlDataReader reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                dailyCards.Add(new DailyCard
                {
                    DailyCardID = reader.GetInt32(0),
                    SeasonID = reader.GetInt32(1),
                    DayNumber = reader.GetInt16(2),
                    NormalCardID = reader.GetInt64(3),
                    PremiumCardAID = reader.GetInt64(4),
                    PremiumCardBID = reader.GetInt64(5),
                    TotalPointsRequired = reader.GetInt32(6)
                });
            }
        }
        return dailyCards;
    }

    static List<SeasonDate> LoadSeasonDates(NpgsqlConnection connection)
    {
        List<SeasonDate> seasonDates = new List<SeasonDate>();
        string query = "SELECT * FROM SeasonDates;";

        using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
        using (NpgsqlDataReader reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                seasonDates.Add(new SeasonDate
                {
                    SeasonID = reader.GetInt32(0),
                    StartDate = reader.GetInt64(1),
                    EndDate = reader.GetInt64(2)
                });
            }
        }
        return seasonDates;
    }
}

public class SeasonInfo
{
    public int SeasonID { get; set; }
    public string SeasonName { get; set; }
    public short SeasonDays { get; set; }
    public long StartDate { get; set; }
    public long EndDate { get; set; }
    public bool IsActive { get; set; }
    public short CurrentLevel { get; set; }
    public int TotalEarnedPoints { get; set; }
    public short NormalLevelsComplete { get; set; }
    public short PremiumLevelsComplete { get; set; }
    public bool EnablePremium { get; set; }
    public short CurrentBuyLevel { get; set; }
    public short SeasonStatus { get; set; }
}

public class SeasonCard
{
    public int CardID { get; set; }
    public int SeasonID { get; set; }
    public long NormalCardID { get; set; }
    public long PremiumCardBID { get; set; }
    public long PremiumCardAID { get; set; }
    public int TotalPointsRequired { get; set; }
}

public class DailyCard
{
    public int DailyCardID { get; set; }
    public int SeasonID { get; set; }
    public short DayNumber { get; set; }
    public long NormalCardID { get; set; }
    public long PremiumCardAID { get; set; }
    public long PremiumCardBID { get; set; }
    public int TotalPointsRequired { get; set; }
}

class SeasonDate
{
    public int SeasonID { get; set; }
    public long StartDate { get; set; }
    public long EndDate { get; set; }
}
