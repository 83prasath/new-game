using System.Numerics;
using Raylib_cs;

namespace SpaceGame
{
    // Struct for enemy projectiles traveling downwards
    public struct EnemyBullet
    {
        public Vector2 Position;
        public float Speed;
        public bool IsActive;
    }

    public class Enemy
    {
        public Vector2 Position;
        public float Speed = 200.0f;
        public int Direction = 1; 
        public int Health = 200;
        public int MaxHealth = 200;
        public bool IsAlive = true;
        
        public float Width = 70.0f;
        public float Height = 50.0f;

        // Enemy Weapon System
        public System.Collections.Generic.List<EnemyBullet> Bullets = new System.Collections.Generic.List<EnemyBullet>();
        private float fireTimer = 0.0f;
        private float fireRateInterval = 1.5f; // Fires a bullet every 1.5 seconds

        public Enemy(Vector2 startPosition)
        {
            Position = startPosition;
        }

        public void Update(float deltaTime, int screenWidth)
        {
            if (!IsAlive) return;

            // 1. Infinite Side-to-Side Patrol Physics
            Position.X += Speed * Direction * deltaTime;
            if (Position.X <= 45) { Position.X = 45; Direction = 1; }
            if (Position.X >= screenWidth - 45) { Position.X = screenWidth - 45; Direction = -1; }

            // 2. Automatic Weapon System Logic
            fireTimer += deltaTime;
            if (fireTimer >= fireRateInterval)
            {
                EnemyBullet newBullet = new EnemyBullet
                {
                    Position = new Vector2(Position.X, Position.Y + 30.0f), // Fires from bottom nose
                    Speed = 400.0f, // Travels downward
                    IsActive = true
                };
                Bullets.Add(newBullet);
                fireTimer = 0.0f;
            }

            // 3. Update Enemy Bullets
            for (int i = 0; i < Bullets.Count; i++)
            {
                EnemyBullet b = Bullets[i];
                if (b.IsActive)
                {
                    b.Position.Y += b.Speed * deltaTime; // Moves DOWNWARD
                    if (b.Position.Y > 768) b.IsActive = false; // Destroy if past screen bottom
                    Bullets[i] = b;
                }
            }
        }

        public void Draw()
        {
            // Even if enemy dies, let its existing bullets finish traveling
            foreach (var bullet in Bullets)
            {
                if (bullet.IsActive)
                {
                    // Crimson enemy laser lines
                    Raylib.DrawRectangleV(bullet.Position, new Vector2(4, 15), Color.RED);
                }
            }

            if (!IsAlive) return;

            // --- ADVANCED GEOMETRY HULL DESIGN ---
            Vector2 nose         = new Vector2(Position.X, Position.Y + 35.0f); // Pointing DOWN
            Vector2 aftLeft      = new Vector2(Position.X - 15.0f, Position.Y - 20.0f);
            Vector2 aftRight     = new Vector2(Position.X + 15.0f, Position.Y - 20.0f);
            Vector2 wingLeftTip  = new Vector2(Position.X - 45.0f, Position.Y - 15.0f);
            Vector2 wingLeftJoin = new Vector2(Position.X - 10.0f, Position.Y + 5.0f);
            Vector2 wingRightTip = new Vector2(Position.X + 45.0f, Position.Y - 15.0f);
            Vector2 wingRightJoin = new Vector2(Position.X + 10.0f, Position.Y + 5.0f);

            // Upward Plasma Trails (Engines)
            Raylib.DrawTriangle(new Vector2(Position.X - 12, Position.Y - 20), new Vector2(Position.X - 8, Position.Y - 35), new Vector2(Position.X - 4, Position.Y - 20), Color.ORANGE);
            Raylib.DrawTriangle(new Vector2(Position.X + 4, Position.Y - 20), new Vector2(Position.X + 8, Position.Y - 35), new Vector2(Position.X + 12, Position.Y - 20), Color.ORANGE);

            // Left & Right Interceptor Wings
            Raylib.DrawTriangle(wingLeftJoin, aftLeft, wingLeftTip, Color.MAROON);
            Raylib.DrawTriangleLines(wingLeftJoin, aftLeft, wingLeftTip, Color.RED);
            Raylib.DrawTriangle(wingRightJoin, wingRightTip, aftRight, Color.MAROON);
            Raylib.DrawTriangleLines(wingRightJoin, wingRightTip, aftRight, Color.RED);

            // Crimson Command Core Hull
            Raylib.DrawTriangle(nose, aftRight, aftLeft, Color.DARKGRAY);
            Raylib.DrawTriangle(nose, new Vector2(Position.X + 10, Position.Y - 10), new Vector2(Position.X - 10, Position.Y - 10), Color.MAROON);
            
            // Cockpit Eye
            Raylib.DrawCircle((int)Position.X, (int)Position.Y + 5, 5, Color.GOLD);

            // High-Tech Health Bar Structure
            float barWidth = 60.0f;
            float healthPercent = (float)Health / MaxHealth;
            Raylib.DrawRectangle((int)(Position.X - barWidth / 2), (int)(Position.Y - 35), (int)barWidth, 5, Color.BLACK);
            Raylib.DrawRectangle((int)(Position.X - barWidth / 2), (int)(Position.Y - 35), (int)(barWidth * healthPercent), 5, Color.RED);
        }
    }
}
