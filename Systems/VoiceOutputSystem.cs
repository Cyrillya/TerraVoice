﻿using Microsoft.Xna.Framework;
using POpusCodec;
using POpusCodec.Enums;
using System;
using Terraria;
using Terraria.ModLoader;
using TerraVoice.IO;

namespace TerraVoice.Core;

[Autoload(Side = ModSide.Client)]
internal sealed class VoiceOutputSystem : ModSystem
{
    public const int MaxProximityRange = 96;

    private static PlayerSpeaker[] playerSpeakers;

    private OpusDecoder decoder;

    public override void PostSetupContent()
    {
        playerSpeakers = new PlayerSpeaker[Main.maxPlayers];

        decoder = new(SamplingRate.Sampling48000, Channels.Mono);
    }

    public override void Unload()
    {
        Main.QueueMainThreadAction(() =>
        {
            foreach (PlayerSpeaker playerSpeaker in playerSpeakers)
            {
                playerSpeaker?.Dispose();
            }

            playerSpeakers = null;
        });
    }

    public void RecieveBuffer(byte[] buffer, int sender)
    {
        short[] samples = decoder.DecodePacket(buffer);

        byte[] decoded = new byte[samples.Length * 2];

        Buffer.BlockCopy(samples, 0, decoded, 0, decoded.Length);

        AddDataToPlayerSpeaker(sender, decoded);
    }

    private void AddDataToPlayerSpeaker(int player, byte[] data)
    {
        playerSpeakers[player] ??= new PlayerSpeaker();
        playerSpeakers[player].SubmitBuffer(data);

        SetPanAndVolume(player);
    }

    private void SetPanAndVolume(int whoAmI)
    {
        UserDataStore data = PersistentDataStoreSystem.GetDataStore<UserDataStore>();

        PlayerSpeaker speaker = playerSpeakers[whoAmI];

        // In tests, pan and volume are irrelevant.
        if (whoAmI == Main.myPlayer)
        { 
            speaker.Volume = 1;
            speaker.Pan = 0;

            return;
        }

        // TODO: Implement individual player volumes.
        if (data.ProximityDistance.Value == 0)
        {
            speaker.Volume = 1;
            speaker.Pan = 0;

            return;
        }

        Player player = Main.player[whoAmI];

        Vector2 screenCenter = Main.screenPosition + Main.ScreenSize.ToVector2() / 2f;

        float playerToCenterX = player.Center.X - screenCenter.X;

        int attenuationDistance = data.ProximityDistance.Value * 16;

        float volume = Math.Clamp(1f - player.Center.Distance(screenCenter) / attenuationDistance, 0f, 1f);

        // TODO: Implement individual player volumes.
        // volume *= 1;

        float pan = Math.Clamp(playerToCenterX / attenuationDistance, -1f, 1f);

        speaker.Volume = volume;
        speaker.Pan = pan;
    }


    [Autoload(Side = ModSide.Client)]
    private class ModPlayerForEnterWorldHook : ModPlayer
    {
        public override void OnEnterWorld()
        {
            foreach (PlayerSpeaker playerSpeaker in playerSpeakers)
            {
                playerSpeaker?.Reset();
            }
        }
    }
}
