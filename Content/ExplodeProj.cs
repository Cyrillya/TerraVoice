using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerraVoice.Misc;

namespace TerraVoice.Content;

public class ExplodeProj : ModProjectile
{
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Dynamite;

    public static Projectile Spawn(Vector2 position, int decibel) {
        if (Main.netMode is NetmodeID.Server) return null;

        float factor = GetDecibelFactor(decibel);
        int damage = (int) MathHelper.Lerp(250, 50000, factor);
        // Main.NewText(damage);
        return Projectile.NewProjectileDirect(new EntitySource_Misc("SpeakingTooLoud"), position, Vector2.Zero,
            ModContent.ProjectileType<ExplodeProj>(), damage, 10f, ai0: decibel);
    }

    private static float GetDecibelFactor(int decibel) {
        // int explodeDecibel = VoiceConfig.Instance.ExplodeDecibel;
        // float factor = Utils.GetLerpValue(explodeDecibel, 115, decibel, clamped: true);
        // factor = MathF.Pow(factor, 2); // 不搞那么复杂了，直接平方曲线
        return Helper.ExplodeMultiplierCurve(decibel); // 最后发现还是用回我最喜欢的曲线吧
    }
    
    private int GetRadius() {
        float factor = GetDecibelFactor(Decibel);
        return (int) MathHelper.Lerp(100, 800, factor);
    }

    public override void SetDefaults() {
        Projectile.width = 2;
        Projectile.height = 2;
        Projectile.hostile = true;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.hide = true;
        Projectile.penetrate = -1;
    }

    public int Decibel {
        get => (int) Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    public override void AI() {
        if (Projectile.timeLeft > 2) {
            Projectile.timeLeft = 2;

            int radius = GetRadius();
            int diameter = radius * 2;
            Projectile.Resize(diameter, diameter);
            Projectile.knockBack = 10f;
        }
    }

    // 爆炸+特效，代码来自ExampleMod
    public override void OnKill(int timeLeft) {
        DoVisualEffects();
        DoExplosion();
    }

    private void DoExplosion() {
        // Finally, actually explode the tiles and walls. Run this code only for the owner
        if (Projectile.owner != Main.myPlayer) return;

        int radius = GetRadius();
        int radiusInTiles = radius / 16 + 1;
        int minTileX = (int) (Projectile.Center.X / 16f - radiusInTiles);
        int maxTileX = (int) (Projectile.Center.X / 16f + radiusInTiles);
        int minTileY = (int) (Projectile.Center.Y / 16f - radiusInTiles);
        int maxTileY = (int) (Projectile.Center.Y / 16f + radiusInTiles);

        // Ensure that all tile coordinates are within the world bounds
        Utils.ClampWithinWorld(ref minTileX, ref minTileY, ref maxTileX, ref maxTileY);

        // These 2 methods handle actually mining the tiles and walls while honoring tile explosion conditions
        bool explodeWalls =
            Projectile.ShouldWallExplode(Projectile.Center, radiusInTiles, minTileX, maxTileX, minTileY, maxTileY);
        Projectile.ExplodeTiles(Projectile.Center, radiusInTiles, minTileX, maxTileX, minTileY, maxTileY, explodeWalls);
    }

    private void DoVisualEffects() {
        // Play explosion sound
        SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
        float factor = GetDecibelFactor(Decibel);
        float amount = MathHelper.Lerp(25, 200, factor);

        // Smoke Dust spawn
        for (int i = 0; i < amount; i++) {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f,
                0f, 100, default, 2f);
            dust.velocity *= 1.4f;
        }

        // Fire Dust spawn
        for (int i = 0; i < amount * 1.6f; i++) {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f,
                0f, 100, default, 3f);
            dust.noGravity = true;
            dust.velocity *= 5f;
            dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f,
                100, default, 2f);
            dust.velocity *= 3f;
        }

        // Large Smoke Gore spawn
        for (int g = 0; g < amount / 20f; g++) {
            var goreSpawnPosition = new Vector2(Projectile.position.X + Projectile.width / 2 - 24f,
                Projectile.position.Y + Projectile.height / 2 - 24f);
            Gore gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default,
                Main.rand.Next(61, 64), 1f);
            gore.scale = 1.5f;
            gore.velocity.X += 1.5f;
            gore.velocity.Y += 1.5f;
            gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default,
                Main.rand.Next(61, 64), 1f);
            gore.scale = 1.5f;
            gore.velocity.X -= 1.5f;
            gore.velocity.Y += 1.5f;
            gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default,
                Main.rand.Next(61, 64), 1f);
            gore.scale = 1.5f;
            gore.velocity.X += 1.5f;
            gore.velocity.Y -= 1.5f;
            gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default,
                Main.rand.Next(61, 64), 1f);
            gore.scale = 1.5f;
            gore.velocity.X -= 1.5f;
            gore.velocity.Y -= 1.5f;
        }
    }
}