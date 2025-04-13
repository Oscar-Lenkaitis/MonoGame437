using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Collections.Generic;
using System.IO;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;

namespace BepuDemo
{
    public class Score
    {
        public int totalShots { get; set; }
        public int currentShots { get; set; }
        public SpriteFont font { get; set; }
        public Vector2 fontPos { get; set; }
        public SpriteBatch spriteBatch { get; set; }
        
        public float powerPercent {get; set; }
        public string powerOutput {get; set; }
        public Vector2 powerPos { get; set; }

        public string output { get; set; }

        public Score()
        {
            totalShots = 0;
            currentShots = 0;
            powerPercent = 0;
            output = $"SHOTS : {currentShots}";
            powerOutput = $"POWER: {powerPercent}%";
        }

        public void hit()
        {
            totalShots++;
            currentShots++;
            output = $"SHOTS : {currentShots}";
            powerOutput = $"POWER : {powerPercent}%";
        }

        public void powerUpdate()
        {
            powerOutput = $"POWER: {powerPercent}%";
        }
        public void reset()
        {
            totalShots = 0;
            currentShots = 0;
            output = $"SHOTS : {currentShots}";
            powerOutput = $"POWER : {powerPercent}%";
        }
    }

    public class ScoreEntry
    {
        public string Initials { get; set; }
        public int Score { get; set; }
    }

 

    public class ScoreManager
    {
        private List<ScoreEntry> scores = new List<ScoreEntry>();

        public void AddScore(ScoreEntry score)
        {
            scores.Add(score);
        }

        public List<ScoreEntry> GetTopScores(int count)
        {
            scores.Sort((a, b) => a.Score.CompareTo(b.Score)); 
            return scores.Take(count).ToList();
        }


        public void LoadScores()
        {
            // Seed with fake data
            scores = new List<ScoreEntry>
            {
                new ScoreEntry { Initials = "KYS", Score = 95 },
                new ScoreEntry { Initials = "NO1", Score = 85 },
                new ScoreEntry { Initials = "CY", Score = 75 },
                new ScoreEntry { Initials = "ISU", Score = 65 },
                new ScoreEntry { Initials = "OIL", Score = 55 }
            };
        }
    }
}
