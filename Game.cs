using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;

namespace SpaceGame
{
    public struct Bullet
    {
        public Vector2 Position;
        public float Speed;
        public bool IsActive;
    }

    public class Game
    {
        public void Run()
        {
            const int screenWidth = 1024;
            const int screenHeight = 768;
            
            Raylib.SetConfigFlags(ConfigFlags.FLAG_MSAA_4X_HINT);
            Raylib.InitWindow(screenWidth, screenHeight, "Space Combat System v3.1");
            Raylib.SetTargetFPS(60);

            // Player Setup
            Vector2 shipPosition = new Vector2(screenWidth / 2.0f, screenHeight - 120.0f);
            float shipSpeed = 450.0f; 
            int playerHealth = 200;
            int playerMaxHealth = 200;
            const float shipWingOffset = 55.0f;

            // Player Weapons
            List<Bullet> playerBullets = new List<Bullet>();
            float bulletSpeed = 600.0f;
            float fireRateCooldown = 0.099f; 
            float fireTimer = 0.0f;

            // Enemy Generation Pipeline
            List<Enemy> enemies = new List<Enemy>();
            float spawnTimer = 0.0f;
            float spawnInterval = 4.0f; // New enemy drops out of warp every 4 seconds

            // First target deployed immediately
            enemies.Add(new Enemy(new Vector2(screenWidth / 2.0f, 120.0f)));

            while (!Raylib.WindowShouldClose())
            {
                float deltaTime = Raylib.GetFrameTime();
                if (fireTimer > 0) fireTimer -= deltaTime;

                // --- 1. ENEMY DEPLOYMENT TIMELINE ---
                spawnTimer += deltaTime;
                if (spawnTimer >= spawnInterval)
                {
                    Random rand = new Random();
                    enemies.Add(new Enemy(new Vector2(rand.Next(100, screenWidth - 100), rand.Next(80, 220))));
                    spawnTimer = 0.0f;
                }

                // --- 2. INPUT MANEUVERING & PRODUCTION ---
                if (playerHealth > 0)
                {
                    if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT) || Raylib.IsKeyDown(KeyboardKey.KEY_A))
                        shipPosition.X -= shipSpeed * deltaTime;
                    if (Raylib.IsKeyDown(KeyboardKey.KEY_RIGHT) || Raylib.IsKeyDown(KeyboardKey.KEY_D))
                        shipPosition.X += shipSpeed * deltaTime;

                    if (Raylib.IsKeyDown(KeyboardKey.KEY_SPACE) && fireTimer <= 0)
                    {
                        playerBullets.Add(new Bullet { Position = new Vector2(shipPosition.X, shipPosition.Y - 60.0f), Speed = bulletSpeed, IsActive = true });
                        fireTimer = fireRateCooldown; 
                    }
                }

                if (shipPosition.X < shipWingOffset) shipPosition.X = shipWingOffset;
                if (shipPosition.X > screenWidth - shipWingOffset) shipPosition.X = screenWidth - shipWingOffset;

                // --- 3. REFRESH HOSTILE STATES ---
                foreach (var enemy in enemies)
                {
                    enemy.Update(deltaTime, screenWidth);

                    // CHECK IF ENEMY BULLETS HIT PLAYER
                    if (playerHealth > 0 && enemy.IsAlive)
                    {
                        for (int k = 0; k < enemy.Bullets.Count; k++)
                        {
                            EnemyBullet eb = enemy.Bullets[k];
                            if (!eb.IsActive) continue;

                            // AABB boundary test against player zone
                            bool hitX = eb.Position.X >= shipPosition.X - 45 && eb.Position.X <= shipPosition.X + 45;
                            bool hitY = eb.Position.Y >= shipPosition.Y - 40 && eb.Position.Y <= shipPosition.Y + 30;

                            if (hitX && hitY)
                            {
                                eb.IsActive = false;
                                playerHealth -= 5; // Fixed: Player health only reduces when hit by ENEMY bullets
                                enemy.Bullets[k] = eb;
                            }
                        }
                    }
                }

                // --- 4. PLAYER PROJECTILE PIPELINE & REGISTRATION ---
                for (int i = 0; i < playerBullets.Count; i++)
                {
                    Bullet b = playerBullets[i];
                    if (!b.IsActive) continue;

                    b.Position.Y -= b.Speed * deltaTime;
                    if (b.Position.Y < -20) b.IsActive = false;
                    else
                    {
                        foreach (var enemy in enemies)
                        {
                            if (enemy.IsAlive)
                            {
                                bool hitX = b.Position.X >= enemy.Position.X - (enemy.Width / 2) && b.Position.X <= enemy.Position.X + (enemy.Width / 2);
                                bool hitY = b.Position.Y >= enemy.Position.Y - (enemy.Height / 2) && b.Position.Y <= enemy.Position.Y + (enemy.Height / 2);

                                if (hitX && hitY)
                                {
                                    b.IsActive = false;
                                    enemy.Health -= 5; // Only reduces enemy health
                                    if (enemy.Health <= 0) enemy.IsAlive = false;
                                    break;
                                }
                            }
                        }
                    }
                    playerBullets[i] = b;
                }

                // --- 5. RENDER PROCESSING STAGE ---
                Raylib.BeginDrawing();
                    Raylib.ClearBackground(new Color(12, 14, 24, 255)); 

                    // Render Player Bullets (Yellow)
                    foreach (var b in playerBullets)
                        if (b.IsActive) Raylib.DrawRectangleV(b.Position, new Vector2(4, 15), Color.YELLOW);

                    // Render All System Enemies
                    foreach (var enemy in enemies) enemy.Draw();

                    // Render High-Tech Player Fighter
                    if (playerHealth > 0)
                    {
                        Vector2 nose         = new Vector2(shipPosition.X, shipPosition.Y - 60.0f);
                        Vector2 aftLeft      = new Vector2(shipPosition.X - 15.0f, shipPosition.Y + 30.0f);
                        Vector2 aftRight     = new Vector2(shipPosition.X + 15.0f, shipPosition.Y + 30.0f);
                        Vector2 wingLeftTip  = new Vector2(shipPosition.X - 55.0f, shipPosition.Y + 25.0f);
                        Vector2 wingLeftJoin = new Vector2(shipPosition.X - 12.0f, shipPosition.Y - 10.0f);
                        Vector2 wingRightTip = new Vector2(shipPosition.X + 55.0f, shipPosition.Y + 25.0f);
                        Vector2 wingRightJoin = new Vector2(shipPosition.X + 12.0f, shipPosition.Y - 10.0f);
                        Vector2 glassNose    = new Vector2(shipPosition.X, shipPosition.Y - 25.0f);
                        Vector2 glassLeft    = new Vector2(shipPosition.X - 7.0f, shipPosition.Y + 5.0f);
                        Vector2 glassRight   = new Vector2(shipPosition.X + 7.0f, shipPosition.Y + 5.0f);

                        Raylib.DrawTriangle(new Vector2(shipPosition.X - 20, shipPosition.Y + 25), new Vector2(shipPosition.X - 15, shipPosition.Y + 55), new Vector2(shipPosition.X - 10, shipPosition.Y + 25), Color.SKYBLUE);
                        Raylib.DrawTriangle(new Vector2(shipPosition.X + 10, shipPosition.Y + 25), new Vector2(shipPosition.X + 15, shipPosition.Y + 55), new Vector2(shipPosition.X + 20, shipPosition.Y + 25), Color.SKYBLUE);
                        Raylib.DrawTriangle(wingLeftJoin, wingLeftTip, aftLeft, Color.DARKGRAY);
                        Raylib.DrawTriangleLines(wingLeftJoin, wingLeftTip, aftLeft, Color.LIME);
                        Raylib.DrawTriangle(wingRightJoin, aftRight, wingRightTip, Color.DARKGRAY);
                        Raylib.DrawTriangleLines(wingRightJoin, aftRight, wingRightTip, Color.LIME);
                        Raylib.DrawTriangle(nose, aftLeft, aftRight, Color.RAYWHITE);
                        Raylib.DrawTriangle(new Vector2(shipPosition.X, shipPosition.Y - 45), aftLeft, aftRight, Color.LIGHTGRAY);
                        Raylib.DrawTriangle(glassNose, glassLeft, glassRight, Color.VIOLET);
                    }
                    else
                    {
                        Raylib.DrawText("HULL INTEGRITY COMPROMISED - GAME OVER", screenWidth / 2 - 240, screenHeight / 2, 24, Color.RED);
                    }

                    // Tactical HUD Layer
                    Raylib.DrawText("SHIELD STABILITY:", 20, 20, 16, Color.RAYWHITE);
                    float healthPercent = Math.Max(0, (float)playerHealth / playerMaxHealth);
                    Raylib.DrawRectangle(20, 45, 200, 20, Color.DARKGRAY);
                    Raylib.DrawRectangle(20, 45, (int)(200 * healthPercent), 20, Color.GREEN);
                    Raylib.DrawText($"{Math.Max(0, playerHealth)} / 200", 90, 48, 14, Color.WHITE);

                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }
    }
}
