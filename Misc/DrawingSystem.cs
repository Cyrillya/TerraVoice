using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Steamworks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using TerraVoice.Core;

namespace TerraVoice.Misc;

[Autoload(Side = ModSide.Client)]
public class DrawingSystem : ModSystem
{
    internal static Queue<float> WaveDatas = new(610);
    internal static int[] PlayerSpeaking = new int[Main.maxPlayers];
    private static float[] _iconOpacity = new float[Main.maxPlayers];
    private static int _iconAnimationTimer = 0;
    internal static float RealVolume { get; set; }
    internal static float CurrentDisplayedVolume;

    public override void UpdateUI(GameTime gameTime) {
        // 支持High FPS Support Mod的高帧率
        double timeFactor = gameTime.ElapsedGameTime.TotalMilliseconds * 0.06;

        float GetVolumePlus(float realVolume, float curDisplayedVolume) =>
            (float) ((realVolume - curDisplayedVolume) * 0.4f * timeFactor);

        CurrentDisplayedVolume += GetVolumePlus(RealVolume, CurrentDisplayedVolume);
        for (var i = 0; i < Main.maxPlayers; i++) {
            var speaker = PlayVoiceSystem.PlayerSpeakers[i];
            if (speaker is null) continue;

            speaker.CurrentDisplayedVolume += GetVolumePlus(speaker.RealVolume, speaker.CurrentDisplayedVolume);
        }
    }

    public override void PostUpdateTime() {
        _iconAnimationTimer++;

        for (var i = 0; i < Main.maxPlayers; i++) {
            ref int speakRemainingTime = ref PlayerSpeaking[i];
            ref float opacity = ref _iconOpacity[i];

            speakRemainingTime--;
            bool speaking = speakRemainingTime > 0;
            if (speaking)
                opacity += 0.12f;
            else
                opacity -= 0.14f;

            opacity = Math.Clamp(opacity, 0f, 1f);
        }
    }

    private void FullscreenMapDraw(Vector2 arg1, float arg2) {
        Main.spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);

        DrawSpeakingPlayers();
        DrawMicrophoneIcon();

        Main.spriteBatch.End();
    }

    private bool DrawWave() {
        if (!PersonalConfig.Instance.ShowWave) return true;

        var datas = WaveDatas.ToArray();
        float x = Main.screenWidth / 2f - 600;
        for (var i = 0; i < datas.Length - 1; i++) {
            var waveData = datas[i];
            var waveDataNext = datas[i + 1];
            float factor = 200f;
            var start = new Vector2(x, Main.screenHeight / 2f + waveData * factor);
            var end = new Vector2(x + 2, Main.screenHeight / 2f + waveDataNext * factor);
            DrawLine(start, end);
            x += 2;
        }

        return true;
    }

    public static void DrawLine(Vector2 start, Vector2 end) {
        float distance = Vector2.Distance(start, end);
        var v = (end - start) / distance;
        var pos = start;
        float rotation = v.ToRotation();
        for (float step = 0.0f; step <= distance; step += 1f) {
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos, new Rectangle(0, 0, 2, 2), Color.White, rotation,
                Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
            pos = start + step * v;
        }
    }

    private bool DrawSpeakingPlayers() {
        // _currentDisplayedVolume += (RealVolume - _currentDisplayedVolume) * 0.4f;

        var speakingTexture = ModAsset.Speaking.Value;
        int frameCount = _iconAnimationTimer / 10 % 3;
        var frame = speakingTexture.Frame(horizontalFrames: 1, verticalFrames: 3, frameX: 0, frameY: frameCount);

        bool isXuansBarDrawn = false; // 第一个音量条是独特的
        var screenCenter = Main.ScreenSize.ToVector2() / 2f;
        var basePosition = new Vector2(screenCenter.X, 8f);
        basePosition.X -= 135f;
        for (var i = 0; i < Main.maxPlayers; i++) {
            var opacity = _iconOpacity[i];
            if (opacity <= 0) continue;

            var player = Main.player[i];
            float playerToCenterX = player.Center.X - (screenCenter.X + Main.screenPosition.X);
            int attenuationDistance = VoiceConfig.Instance.VoiceAttenuationDistance * 16;
            bool tooFarToHear = Math.Abs(playerToCenterX) > attenuationDistance;
            if (VoiceConfig.Instance.VoiceAttenuation && tooFarToHear && i != Main.myPlayer) continue;

            var position = basePosition;
            DrawPlayerHead(player, position, opacity, 0.8f, Color.White);

            position.X += 32f;
            float textMaxSize = 90f;
            var playerName = player.name;
            string displayedName = "";
            for (var j = 0; j < playerName.Length; j++) {
                displayedName += playerName[j];
                if (i < playerName.Length - 2 &&
                    FontAssets.MouseText.Value.MeasureString(displayedName + "…").X > textMaxSize - 6f) {
                    displayedName += "…";
                    break;
                }
            }
            
            // 调试用
            // var speaker = PlayVoiceSystem.PlayerSpeakers[i];
            // if (speaker is not null) {
            //     float volume = i == Main.myPlayer ? CurrentDisplayedVolume : speaker.CurrentDisplayedVolume;
            //     float db = Helper.GetDecibel(volume);
            //     displayedName = db.ToString();
            // }

            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value, displayedName,
                position,
                Color.White * opacity, 0f, Vector2.Zero, Vector2.One, spread: 1f);

            position.X += 94f;
            var iconColor = Color.White;
            iconColor *= opacity;
            Main.spriteBatch.Draw(speakingTexture, position, frame, iconColor, 0f, Vector2.Zero, 1f, SpriteEffects.None,
                0f);

            position.X += frame.Width + 4;
            var barTexture = !isXuansBarDrawn ? ModAsset.VolumeBarXuan.Value : ModAsset.VolumeBar.Value;
            isXuansBarDrawn = true;
            DrawVolumeBar(i, barTexture, position);

            basePosition.Y += 30f;
        }

        return true;
    }

    private void DrawVolumeBar(int whoAmI, Texture2D barTexture, Vector2 position) {
        PlayVoiceSystem.PlayerSpeakers[whoAmI] ??= new PlayerSpeaker();
        var speaker = PlayVoiceSystem.PlayerSpeakers[whoAmI];
        float volume = whoAmI == Main.myPlayer ? CurrentDisplayedVolume : speaker.CurrentDisplayedVolume;
        volume = Math.Clamp(volume * 2.7f, 0f, 1f);

        var volumeFilledFrame = barTexture.Frame(horizontalFrames: 1, verticalFrames: 2, frameX: 0, frameY: 0);
        var volumeEmptyFrame = barTexture.Frame(horizontalFrames: 1, verticalFrames: 2, frameX: 0, frameY: 1);
        volumeFilledFrame.Width = (int) (volumeEmptyFrame.Width * volume);
        Main.spriteBatch.Draw(barTexture, position, volumeEmptyFrame, Color.White, 0f, Vector2.Zero, 1f,
            SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(barTexture, position, volumeFilledFrame, Color.White, 0f, Vector2.Zero, 1f,
            SpriteEffects.None, 0f);
    }

    private bool DrawMicrophoneIcon() {
        if (!PersonalConfig.Instance.MicrophoneIcon) return true;
        // 不是按住说话模式，或开启了声音衰减，就会显示图标
        if (!VoiceConfig.Instance.VoiceAttenuation && PersonalConfig.Instance.TalkMode is TalkMode.Push) return true;

        var tex = ModAsset.Microphone;
        int frameCount = 0;
        if (SteamUser.GetAvailableVoice(out _) is EVoiceResult.k_EVoiceResultNotRecording)
            frameCount = 1;
        var frame = tex.Frame(horizontalFrames: 1, verticalFrames: 2, frameX: 0, frameY: frameCount);
        var position = Main.ScreenSize.ToVector2() - frame.Size() - new Vector2(6f);
        Main.spriteBatch.Draw(tex.Value, position, frame, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

        return true;
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
        var speakingPlayersLayer =
            new LegacyGameInterfaceLayer("TerraVoice: Speaking Players", DrawSpeakingPlayers, InterfaceScaleType.UI);
        var microphoneIconLayer =
            new LegacyGameInterfaceLayer("TerraVoice: Microphone Icon", DrawMicrophoneIcon, InterfaceScaleType.UI);
        var waveLayer =
            new LegacyGameInterfaceLayer("TerraVoice: Sound Wave", DrawWave, InterfaceScaleType.UI);

        int index = layers.FindIndex(l => l.Name is "Vanilla: Player Chat");
        if (index != -1)
            layers.InsertRange(index, new[] {speakingPlayersLayer, microphoneIconLayer, waveLayer});
    }

    public override void Load() {
        Main.OnPostFullscreenMapDraw += FullscreenMapDraw;
    }

    public override void Unload() {
        Main.OnPostFullscreenMapDraw -= FullscreenMapDraw;
    }

    public void DrawPlayerHead(Player drawPlayer, Vector2 position, float opacity = 1f, float scale = 1f,
        Color borderColor = default) {
        var playerHeadDrawRenderTargetContent = Main.MapPlayerRenderer._playerRenders[drawPlayer.whoAmI];
        playerHeadDrawRenderTargetContent.UsePlayer(drawPlayer);
        playerHeadDrawRenderTargetContent.UseColor(borderColor);
        playerHeadDrawRenderTargetContent.Request();
        Main.MapPlayerRenderer._anyDirty = true;
        Main.MapPlayerRenderer._drawData.Clear();
        if (!playerHeadDrawRenderTargetContent.IsReady) return;

        var target = playerHeadDrawRenderTargetContent.GetTarget();
        var origin = target.Size() / 2f;
        position += new Vector2(11f, 8f);
        Main.MapPlayerRenderer._drawData.Add(new DrawData(target, position, null, Color.White * opacity, 0f,
            origin, scale, SpriteEffects.None));
        Main.MapPlayerRenderer.RenderDrawData(drawPlayer);
    }
}