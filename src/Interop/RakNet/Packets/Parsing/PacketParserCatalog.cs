namespace SFSharp;

internal delegate TPayload ArizonaReaderParser<TPayload>(ref BitStreamReader reader);

public static class PacketParserCatalog
{
    public static PacketParserRegistry CreateDefaultRegistry()
    {
        PacketParserRegistry registry = new();
        RegisterSync(registry);
        RegisterArizona220(registry);
        RegisterArizona221(registry);
        return registry;
    }

    private static void RegisterSync(PacketParserRegistry registry)
    {
        registry.Register(new DelegateIncomingPacketParser<IncomingConnectionRequestAcceptedPacket>(PacketId.ConnectionRequestAccepted, ParseIncomingConnectionRequestAccepted, exactBitLength: 104));
        registry.Register(new DelegateIncomingPacketParser<IncomingConnectionAttemptFailedPacket>(PacketId.ConnectionAttemptFailed, ParseIncomingConnectionAttemptFailed, exactBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingNoFreeIncomingConnectionsPacket>(PacketId.NoFreeIncomingConnections, ParseIncomingNoFreeIncomingConnections, exactBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingDisconnectionNotificationPacket>(PacketId.DisconnectionNotification, ParseIncomingDisconnectionNotification, exactBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingAuthenticationPacket>(PacketId.Authentication, ParseIncomingAuthentication, minimumBitLength: 16));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingAuthenticationPacket>(PacketId.Authentication, ParseOutgoingAuthentication, minimumBitLength: 16));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingRconCommandPacket>(PacketId.RconCommand, ParseOutgoingRconCommand, minimumBitLength: 40));
        registry.Register(new DelegateIncomingPacketParser<IncomingConnectionLostPacket>(PacketId.ConnectionLost, ParseIncomingConnectionLost, exactBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingConnectionBannedPacket>(PacketId.ConnectionBanned, ParseIncomingConnectionBanned, exactBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingInvalidPasswordPacket>(PacketId.InvalidPassword, ParseIncomingInvalidPassword, exactBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingConnectionFailedPacket>(PacketId.ConnectionFailed, ParseIncomingConnectionFailed, exactBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingOnfootPacket>(PacketId.OnfootData, ParseIncomingOnfoot, minimumBitLength: 24));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingOnfootPacket>(PacketId.OnfootData, ParseOutgoingOnfoot, minimumBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingIncarPacket>(PacketId.IncarData, ParseIncomingIncar, minimumBitLength: 24));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingIncarPacket>(PacketId.IncarData, ParseOutgoingIncar, minimumBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingAimPacket>(PacketId.AimData, ParseIncomingAim, minimumBitLength: 24, exactBitLength: 272));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingAimPacket>(PacketId.AimData, ParseOutgoingAim, minimumBitLength: 8, exactBitLength: 256));
        registry.Register(new DelegateIncomingPacketParser<IncomingBulletPacket>(PacketId.BulletData, ParseIncomingBullet, minimumBitLength: 24, exactBitLength: 344));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingBulletPacket>(PacketId.BulletData, ParseOutgoingBullet, minimumBitLength: 8, exactBitLength: 328));
        registry.Register(new DelegateIncomingPacketParser<IncomingPassengerPacket>(PacketId.PassengerData, ParseIncomingPassenger, minimumBitLength: 24, exactBitLength: 216));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingPassengerPacket>(PacketId.PassengerData, ParseOutgoingPassenger, minimumBitLength: 8, exactBitLength: 200));
        registry.Register(new DelegateIncomingPacketParser<IncomingUnoccupiedPacket>(PacketId.UnoccupiedData, ParseIncomingUnoccupied, minimumBitLength: 24, exactBitLength: 560));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingUnoccupiedPacket>(PacketId.UnoccupiedData, ParseOutgoingUnoccupied, minimumBitLength: 8, exactBitLength: 544));
        registry.Register(new DelegateIncomingPacketParser<IncomingTrailerPacket>(PacketId.TrailerData, ParseIncomingTrailer, minimumBitLength: 24, exactBitLength: 456));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingTrailerPacket>(PacketId.TrailerData, ParseOutgoingTrailer, minimumBitLength: 8, exactBitLength: 440));
        registry.Register(new DelegateIncomingPacketParser<IncomingSpectatorPacket>(PacketId.SpectatorData, ParseIncomingSpectator, minimumBitLength: 24));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingSpectatorPacket>(PacketId.SpectatorData, ParseOutgoingSpectator, minimumBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingWeaponsPacket>(PacketId.WeaponsData, ParseIncomingWeapons, minimumBitLength: 24));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingWeaponsPacket>(PacketId.WeaponsData, ParseOutgoingWeapons, minimumBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingStatsPacket>(PacketId.StatsData, ParseIncomingStats, minimumBitLength: 24));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingStatsPacket>(PacketId.StatsData, ParseOutgoingStats, minimumBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingMarkersPacket>(PacketId.MarkersData, ParseIncomingMarkers, minimumBitLength: 16));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingMarkersPacket>(PacketId.MarkersData, ParseOutgoingMarkers, minimumBitLength: 8));
    }

    private static void RegisterArizona220(PacketParserRegistry registry)
    {
        Register220Incoming(registry, ArizonaPacketId.SetLocalDriver, ArizonaPacket.ParseSetLocalDriver);
        Register220Incoming(registry, ArizonaPacketId.TurnLightUpdate, ArizonaPacket.ParseTurnLightUpdate);
        Register220Incoming(registry, ArizonaPacketId.SetSatiety, ArizonaPacket.ParseSetSatiety);
        Register220Incoming(registry, ArizonaPacketId.SetHudMode, ArizonaPacket.ParseSetHudMode);
        Register220Incoming(registry, ArizonaPacketId.SetRadarMode, ArizonaPacket.ParseSetRadarMode);
        Register220Incoming(registry, ArizonaPacketId.LoadJs, ArizonaPacket.ParseLoadJs);
        Register220Incoming(registry, ArizonaPacketId.PlayMediaOnBillboard, ArizonaPacket.ParsePlayMediaOnBillboard);
        Register220Incoming(registry, ArizonaPacketId.LoadHtml, ArizonaPacket.ParseLoadHtml);
        Register220Incoming(registry, ArizonaPacketId.InjectCode, ArizonaPacket.ParseInjectCode);
        Register220Incoming(registry, ArizonaPacketId.ToggleCursor, ArizonaPacket.ParseToggleCursor);
        Register220Incoming(registry, ArizonaPacketId.SetPlayerUnknownState, ArizonaPacket.ParseSetPlayerUnknownState);
        Register220Incoming(registry, ArizonaPacketId.UiColorScale, ArizonaPacket.ParseUiColorScale);
        Register220Incoming(registry, ArizonaPacketId.SetChatGroup, ArizonaPacket.ParseSetChatGroup);
        Register220Incoming(registry, ArizonaPacketId.SetLocalInVehicle, ArizonaPacket.ParseSetLocalInVehicle);
        Register220Incoming(registry, ArizonaPacketId.SetNicknameMode, ArizonaPacket.ParseSetNicknameMode);
        Register220Incoming(registry, ArizonaPacketId.SwitchChatMode, ArizonaPacket.ParseSwitchChatMode);
        Register220Incoming(registry, ArizonaPacketId.SetVisibleDistance3DMarker, ArizonaPacket.ParseSetVisibleDistance3DMarker);
        Register220Incoming(registry, ArizonaPacketId.ShowPositionInDiscord, ArizonaPacket.ParseShowPositionInDiscord);
        Register220Incoming(registry, ArizonaPacketId.Unknown86, ArizonaPacket.ParseUnknown86);
        Register220Incoming(registry, ArizonaPacketId.AutoDrinkBeer, ArizonaPacket.ParseAutoDrinkBeer);
        Register220Incoming(registry, ArizonaPacketId.SetDayNightColors, ArizonaPacket.ParseSetDayNightColors);
        Register220Incoming(registry, ArizonaPacketId.ToggleCompass, ArizonaPacket.ParseToggleCompass);
        Register220Incoming(registry, ArizonaPacketId.SetAnimationProperty, ArizonaPacket.ParseSetAnimationProperty);
        Register220Incoming(registry, ArizonaPacketId.ToggleMapColors, ArizonaPacket.ParseToggleMapColors);
        Register220Incoming(registry, ArizonaPacketId.ToggleUnknown102, ArizonaPacket.ParseToggleUnknown102);
        Register220Incoming(registry, ArizonaPacketId.ChangeServer, ArizonaPacket.ParseChangeServer);
        Register220Incoming(registry, ArizonaPacketId.ShowLoadScreenVc, ArizonaPacket.ParseShowLoadScreenVc);
        Register220Incoming(registry, ArizonaPacketId.ToggleUnknown105, ArizonaPacket.ParseToggleUnknown105);
        registry.Register(new DelegateIncomingArizonaPacketParser<IncomingArizonaPacket<ArzSwitchChatState>>(PacketId.ArizonaCef, (int)ArizonaPacketId.SwitchChatState, args => ParseIncoming220(args, ArizonaPacketId.SwitchChatState, ArizonaPacket.ParseSwitchChatState), name: $"Arizona220:{ArizonaPacketId.SwitchChatState}", minimumPayloadBitLength: 33));
        Register220Incoming(registry, ArizonaPacketId.UiConfig, ArizonaPacket.ParseUiConfig);
        Register220Incoming(registry, ArizonaPacketId.SetSpectatorPatches, ArizonaPacket.ParseSetSpectatorPatches);
        Register220Incoming(registry, ArizonaPacketId.ToggleUnknown114, ArizonaPacket.ParseToggleUnknown114);
        Register220Incoming(registry, ArizonaPacketId.SetViceCityFlag, ArizonaPacket.ParseSetViceCityFlag);
        Register220Incoming(registry, ArizonaPacketId.SetPlayerNametagFlags, ArizonaPacket.ParseSetPlayerNametagFlags);
        Register220Incoming(registry, ArizonaPacketId.SetMapIcon, ArizonaPacket.ParseSetMapIcon);
        Register220Incoming(registry, ArizonaPacketId.UiScalar, ArizonaPacket.ParseUiScalar);
        Register220Incoming(registry, ArizonaPacketId.SetVehicleColorSmoke, ArizonaPacket.ParseSetVehicleColorSmoke);
        Register220Incoming(registry, ArizonaPacketId.VehicleColor, ArizonaPacket.ParseVehicleColor);
        Register220Incoming(registry, ArizonaPacketId.SetSkyboxImages, ArizonaPacket.ParseSetSkyboxImages);
        Register220Incoming(registry, ArizonaPacketId.Create3DWaypoint, ArizonaPacket.ParseCreate3DWaypoint);
        Register220Incoming(registry, ArizonaPacketId.SetHudStyle, ArizonaPacket.ParseSetHudStyle);
        Register220Incoming(registry, ArizonaPacketId.ToggleRenderTarget, ArizonaPacket.ParseToggleRenderTarget);
        Register220Incoming(registry, ArizonaPacketId.SetVehicleNumberPlate, ArizonaPacket.ParseSetVehicleNumberPlate);
        Register220Incoming(registry, ArizonaPacketId.SetPlayerAttachedObject, ArizonaPacket.ParseSetPlayerAttachedObject);
        Register220Incoming(registry, ArizonaPacketId.LoadBinary, ArizonaPacket.ParseLoadBinary);
        Register220Incoming(registry, ArizonaPacketId.TogglePortal, ArizonaPacket.ParseTogglePortal);
        Register220Incoming(registry, ArizonaPacketId.CreatePortal, ArizonaPacket.ParseCreatePortal);
        Register220Incoming(registry, ArizonaPacketId.DestroyPortal, ArizonaPacket.ParseDestroyPortal);
        Register220Incoming(registry, ArizonaPacketId.ToggleUnknown163, ArizonaPacket.ParseToggleUnknown163);
        Register220Incoming(registry, ArizonaPacketId.ToggleUnknown164, ArizonaPacket.ParseToggleUnknown164);
        Register220Incoming(registry, ArizonaPacketId.SetCurrentTask, ArizonaPacket.ParseSetCurrentTask);
        Register220Incoming(registry, ArizonaPacketId.ToggleDrawInterface, ArizonaPacket.ParseToggleDrawInterface);
        Register220Incoming(registry, ArizonaPacketId.SetInterior, ArizonaPacket.ParseSetInterior);
        Register220Incoming(registry, ArizonaPacketId.UiToggle, ArizonaPacket.ParseUiToggle);
        Register220Incoming(registry, ArizonaPacketId.VehicleHeadlightsState, ArizonaPacket.ParseVehicleHeadlightsState);
        Register220Incoming(registry, ArizonaPacketId.SetVirtualWorld, ArizonaPacket.ParseSetVirtualWorld);
        Register220Incoming(registry, ArizonaPacketId.SetVehicleDriftMode, ArizonaPacket.ParseSetVehicleDriftMode);
        Register220Incoming(registry, ArizonaPacketId.SetVehicleLights, ArizonaPacket.ParseSetVehicleLights);
        Register220Incoming(registry, ArizonaPacketId.SetPlayerSkin, ArizonaPacket.ParseSetPlayerSkin);
        Register220Incoming(registry, ArizonaPacketId.SetVehicleStrobelights, ArizonaPacket.ParseSetVehicleStrobelights);
        Register220Incoming(registry, ArizonaPacketId.SetGpsRoute, ArizonaPacket.ParseSetGpsRoute);
        Register220Incoming(registry, ArizonaPacketId.SetFirstPersonCamera, ArizonaPacket.ParseSetFirstPersonCamera);

        Register220Incoming(registry, ArizonaPacketId.Unknown11, ArizonaPacket.ParseCustomUnknown11, "Unknown11");
        Register220Incoming(registry, ArizonaPacketId.Unknown13, ArizonaPacket.ParseCustomUnknown13, "Unknown13");
        Register220Incoming(registry, ArizonaPacketId.Close, ArizonaPacket.ParseCustomClose, "Close");
        Register220Incoming(registry, ArizonaPacketId.Move, ArizonaPacket.ParseCustomMove, "Move");
        Register220Incoming(registry, ArizonaPacketId.ToggleScreen, ArizonaPacket.ParseCustomToggleScreen, "ToggleScreen");
        Register220Incoming(registry, ArizonaPacketId.ModuleReadRequest, ArizonaPacket.ParseCustomModuleReadRequest, "ModuleReadRequest");
        Register220Incoming(registry, ArizonaPacketId.ToggleShow, ArizonaPacket.ParseCustomToggleShow, "ToggleShow");
        Register220Incoming(registry, ArizonaPacketId.GetBrowserControlState, ArizonaPacket.ParseCustomGetBrowserControlState, "GetBrowserControlState");
        Register220Incoming(registry, ArizonaPacketId.Resize, ArizonaPacket.ParseCustomResize, "Resize");
        Register220Incoming(registry, ArizonaPacketId.AddObject, ArizonaPacket.ParseCustomAddObject, "AddObject");
        Register220Incoming(registry, ArizonaPacketId.RemoveObject, ArizonaPacket.ParseCustomRemoveObject, "RemoveObject");
        Register220Incoming(registry, ArizonaPacketId.BlipIcon, ArizonaPacket.ParseCustomBlipIconRaw, "BlipIcon");
        Register220Incoming(registry, ArizonaPacketId.MarkerIconBatch, ArizonaPacket.ParseCustomMarkerIconBatchRaw, "MarkerIconBatch");

        Register220Outgoing(registry, ArizonaPacketId.SendKey, ArizonaPacket.ParseSendKey);
        Register220Outgoing(registry, ArizonaPacketId.SendSwitchChatState, ArizonaPacket.ParseSendSwitchChatState);
        Register220Outgoing(registry, ArizonaPacketId.SendTurnLights, ArizonaPacket.ParseSendTurnLights);
        Register220Outgoing(registry, ArizonaPacketId.SendOpenInterface, ArizonaPacket.ParseSendOpenInterface);
        Register220Outgoing(registry, ArizonaPacketId.Send, ArizonaPacket.ParseSendText);
        Register220Outgoing(registry, ArizonaPacketId.SendResolution, ArizonaPacket.ParseSendResolution);
        Register220Outgoing(registry, ArizonaPacketId.SendToggleDrawInterface, ArizonaPacket.ParseSendToggleDrawInterface);
        Register220Outgoing(registry, ArizonaPacketId.SendHash, ArizonaPacket.ParseSendHash);
        Register220Outgoing(registry, ArizonaPacketId.SendSwitchChatMode, ArizonaPacket.ParseSendSwitchChatMode);
        Register220Outgoing(registry, ArizonaPacketId.SendFloatValue, ArizonaPacket.ParseSendFloatValue);
        Register220Outgoing(registry, ArizonaPacketId.SendToggleActionState, ArizonaPacket.ParseSendToggleActionState);
        Register220Outgoing(registry, ArizonaPacketId.SendTargetPosition, ArizonaPacket.ParseSendTargetPosition);
        Register220Outgoing(registry, ArizonaPacketId.SendClientJoin, ArizonaPacket.ParseSendClientJoin);
        Register220Outgoing(registry, ArizonaPacketId.SendDroneHeading, ArizonaPacket.ParseSendDroneHeading);
        Register220Outgoing(registry, ArizonaPacketId.SendPortalToggle, ArizonaPacket.ParseSendPortalToggle);
        Register220Outgoing(registry, ArizonaPacketId.SendWeaponScroll, ArizonaPacket.ParseSendWeaponScroll);
        Register220Outgoing(registry, ArizonaPacketId.SendDamageResponseWeapon, ArizonaPacket.ParseSendDamageResponseWeapon);

        Register220Outgoing(registry, ArizonaPacketId.ModuleReadRequest, ArizonaPacket.ParseCustomModuleReadRequest, "ModuleReadRequest");
        Register220Outgoing(registry, ArizonaPacketId.BrowserClick, ArizonaPacket.ParseCustomBrowserClick, "BrowserClick");
        Register220Outgoing(registry, ArizonaPacketId.SetBrowserControlState, ArizonaPacket.ParseCustomSetBrowserControlState, "SetBrowserControlState");
    }

    private static void RegisterArizona221(PacketParserRegistry registry)
    {
        Register221Incoming(registry, ArizonaPacketIdEx.BotStreamIn, ArizonaPacket.ParseBotStreamIn);
        Register221Incoming(registry, ArizonaPacketIdEx.BotStreamOut, ArizonaPacket.ParseBotStreamOut);
        Register221Incoming(registry, ArizonaPacketIdEx.BotOnfootSync, ArizonaPacket.ParseBotOnfootSync);
        Register221Incoming(registry, ArizonaPacketIdEx.SetBotInvulnerable, ArizonaPacket.ParseSetBotInvulnerable);
        Register221Incoming(registry, ArizonaPacketIdEx.SetBotName, ArizonaPacket.ParseSetBotName);
        Register221Incoming(registry, ArizonaPacketIdEx.SetBotWeapon, ArizonaPacket.ParseSetBotWeapon);
        Register221Incoming(registry, ArizonaPacketIdEx.SetBotPos, ArizonaPacket.ParseSetBotPos);
        Register221Incoming(registry, ArizonaPacketIdEx.MoveBotToPos, ArizonaPacket.ParseMoveBotToPos);
        Register221Incoming(registry, ArizonaPacketIdEx.ApplyBotAnimation, ArizonaPacket.ParseApplyBotAnimation);
        Register221Incoming(registry, ArizonaPacketIdEx.BotAttackPlayer, ArizonaPacket.ParseBotAttackPlayer);
        Register221Incoming(registry, ArizonaPacketIdEx.BotEnterVehicle, ArizonaPacket.ParseBotEnterVehicle);
        Register221Incoming(registry, ArizonaPacketIdEx.BotPassengerSync, ArizonaPacket.ParseBotPassengerSync);
        Register221Incoming(registry, ArizonaPacketIdEx.BotExitVehicle, ArizonaPacket.ParseBotExitVehicle);
        Register221Incoming(registry, ArizonaPacketIdEx.BotChatBubble, ArizonaPacket.ParseBotChatBubble);
        Register221Incoming(registry, ArizonaPacketIdEx.SetBotAttachedObject, ArizonaPacket.ParseSetBotAttachedObject);
        Register221Incoming(registry, ArizonaPacketIdEx.RemoveBotAttachedObject, ArizonaPacket.ParseRemoveBotAttachedObject);
        Register221Incoming(registry, ArizonaPacketIdEx.ShootBotAtBot, ArizonaPacket.ParseShootBotAtBot);
        Register221Incoming(registry, ArizonaPacketIdEx.DestroyBot, ArizonaPacket.ParseDestroyBot);
        Register221Incoming(registry, ArizonaPacketIdEx.SetBotAttachedSimpleObject, ArizonaPacket.ParseSetBotAttachedSimpleObject);

        Register221Outgoing(registry, ArizonaPacketIdEx.SendBotOnfootSync, ArizonaPacket.ParseSendBotOnfootSync);
        Register221Outgoing(registry, ArizonaPacketIdEx.SendBotDamage, ArizonaPacket.ParseSendBotDamage);
    }

    private static IncomingConnectionAttemptFailedPacket ParseIncomingConnectionAttemptFailed(IncomingPacketArgs args)
    {
        return new IncomingConnectionAttemptFailedPacket();
    }

    private static IncomingNoFreeIncomingConnectionsPacket ParseIncomingNoFreeIncomingConnections(IncomingPacketArgs args)
    {
        return new IncomingNoFreeIncomingConnectionsPacket();
    }

    private static IncomingDisconnectionNotificationPacket ParseIncomingDisconnectionNotification(IncomingPacketArgs args)
    {
        return new IncomingDisconnectionNotificationPacket();
    }
    private static IncomingConnectionRequestAcceptedPacket ParseIncomingConnectionRequestAccepted(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        int ip = reader.ReadInt32();
        ushort port = reader.ReadUInt16();
        ushort playerId = reader.ReadUInt16();
        int challenge = reader.ReadInt32();
        return new IncomingConnectionRequestAcceptedPacket(ip, port, playerId, challenge);
    }

    private static IncomingAuthenticationPacket ParseIncomingAuthentication(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new IncomingAuthenticationPacket(reader.ReadStringUInt8Length());
    }

    private static OutgoingAuthenticationPacket ParseOutgoingAuthentication(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingAuthenticationPacket(reader.ReadStringUInt8Length());
    }

    private static OutgoingRconCommandPacket ParseOutgoingRconCommand(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingRconCommandPacket(reader.ReadStringUInt32Length());
    }

    private static IncomingConnectionLostPacket ParseIncomingConnectionLost(IncomingPacketArgs args)
    {
        return new IncomingConnectionLostPacket();
    }

    private static IncomingConnectionBannedPacket ParseIncomingConnectionBanned(IncomingPacketArgs args)
    {
        return new IncomingConnectionBannedPacket();
    }

    private static IncomingInvalidPasswordPacket ParseIncomingInvalidPassword(IncomingPacketArgs args)
    {
        return new IncomingInvalidPasswordPacket();
    }

    private static IncomingConnectionFailedPacket ParseIncomingConnectionFailed(IncomingPacketArgs args)
    {
        return new IncomingConnectionFailedPacket();
    }
    private static IncomingOnfootPacket ParseIncomingOnfoot(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        ushort playerId = reader.ReadUInt16();
        return new IncomingOnfootPacket(playerId, OnfootSyncData.Parse(ref reader));
    }

    private static OutgoingOnfootPacket ParseOutgoingOnfoot(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingOnfootPacket(OutgoingOnfootSyncData.Parse(ref reader));
    }

    private static IncomingIncarPacket ParseIncomingIncar(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        ushort playerId = reader.ReadUInt16();
        return new IncomingIncarPacket(playerId, IncarSyncData.Parse(ref reader));
    }

    private static OutgoingIncarPacket ParseOutgoingIncar(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingIncarPacket(OutgoingIncarSyncData.Parse(ref reader));
    }

    private static IncomingAimPacket ParseIncomingAim(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        ushort playerId = reader.ReadUInt16();
        return new IncomingAimPacket(playerId, AimSyncData.Parse(ref reader));
    }

    private static OutgoingAimPacket ParseOutgoingAim(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingAimPacket(AimSyncData.Parse(ref reader));
    }

    private static IncomingBulletPacket ParseIncomingBullet(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        ushort playerId = reader.ReadUInt16();
        return new IncomingBulletPacket(playerId, BulletSyncData.Parse(ref reader));
    }

    private static OutgoingBulletPacket ParseOutgoingBullet(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingBulletPacket(BulletSyncData.Parse(ref reader));
    }

    private static IncomingPassengerPacket ParseIncomingPassenger(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        ushort playerId = reader.ReadUInt16();
        return new IncomingPassengerPacket(playerId, PassengerSyncData.Parse(ref reader));
    }

    private static OutgoingPassengerPacket ParseOutgoingPassenger(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingPassengerPacket(PassengerSyncData.Parse(ref reader));
    }

    private static IncomingUnoccupiedPacket ParseIncomingUnoccupied(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        ushort playerId = reader.ReadUInt16();
        return new IncomingUnoccupiedPacket(playerId, UnoccupiedSyncData.Parse(ref reader));
    }

    private static OutgoingUnoccupiedPacket ParseOutgoingUnoccupied(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingUnoccupiedPacket(UnoccupiedSyncData.Parse(ref reader));
    }

    private static IncomingTrailerPacket ParseIncomingTrailer(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        ushort playerId = reader.ReadUInt16();
        return new IncomingTrailerPacket(playerId, TrailerSyncData.Parse(ref reader));
    }

    private static OutgoingTrailerPacket ParseOutgoingTrailer(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingTrailerPacket(TrailerSyncData.Parse(ref reader));
    }

    private static IncomingSpectatorPacket ParseIncomingSpectator(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        ushort playerId = reader.ReadUInt16();
        return new IncomingSpectatorPacket(playerId, SpectatorSyncData.Parse(ref reader));
    }

    private static OutgoingSpectatorPacket ParseOutgoingSpectator(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingSpectatorPacket(SpectatorSyncData.Parse(ref reader));
    }

    private static IncomingWeaponsPacket ParseIncomingWeapons(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        ushort playerId = reader.ReadUInt16();
        return new IncomingWeaponsPacket(playerId, WeaponsSyncData.Parse(ref reader));
    }

    private static OutgoingWeaponsPacket ParseOutgoingWeapons(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingWeaponsPacket(WeaponsSyncData.Parse(ref reader));
    }

    private static IncomingStatsPacket ParseIncomingStats(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        ushort playerId = reader.ReadUInt16();
        return new IncomingStatsPacket(playerId, StatsSyncData.Parse(ref reader));
    }

    private static OutgoingStatsPacket ParseOutgoingStats(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingStatsPacket(StatsSyncData.Parse(ref reader));
    }

    private static IncomingMarkersPacket ParseIncomingMarkers(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        byte markerSource = reader.ReadUInt8();
        return new IncomingMarkersPacket(markerSource, MarkersSyncData.Parse(ref reader));
    }

    private static OutgoingMarkersPacket ParseOutgoingMarkers(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingMarkersPacket(MarkersSyncData.Parse(ref reader));
    }

    private static void Register220Incoming<TPayload>(PacketParserRegistry registry, ArizonaPacketId subId, ArizonaReaderParser<TPayload> parser, string? name = null)
    {
        string packetName = name ?? subId.ToString();
        registry.Register(new DelegateIncomingArizonaPacketParser<IncomingArizonaPacket<TPayload>>(PacketId.ArizonaCef, (int)subId, args => ParseIncoming220(args, subId, parser), name: $"Arizona220:{packetName}"));
    }

    private static void Register220Outgoing<TPayload>(PacketParserRegistry registry, ArizonaPacketId subId, ArizonaReaderParser<TPayload> parser, string? name = null)
    {
        string packetName = name ?? subId.ToString();
        registry.Register(new DelegateOutgoingArizonaPacketParser<OutgoingArizonaPacket<TPayload>>(PacketId.ArizonaCef, (int)subId, args => ParseOutgoing220(args, subId, parser), name: $"Arizona220:{packetName}"));
    }

    private static void Register221Incoming<TPayload>(PacketParserRegistry registry, ArizonaPacketIdEx subId, ArizonaReaderParser<TPayload> parser)
    {
        registry.Register(new DelegateIncomingArizonaPacketParser<IncomingArizonaPacket<TPayload>>(PacketId.ArizonaCefEx, (int)subId, args => ParseIncoming221(args, subId, parser), name: $"Arizona221:{subId}"));
    }

    private static void Register221Outgoing<TPayload>(PacketParserRegistry registry, ArizonaPacketIdEx subId, ArizonaReaderParser<TPayload> parser)
    {
        registry.Register(new DelegateOutgoingArizonaPacketParser<OutgoingArizonaPacket<TPayload>>(PacketId.ArizonaCefEx, (int)subId, args => ParseOutgoing221(args, subId, parser), name: $"Arizona221:{subId}"));
    }

    private static IncomingArizonaPacket<TPayload> ParseIncoming220<TPayload>(IncomingArizonaPacketArgs args, ArizonaPacketId subId, ArizonaReaderParser<TPayload> parser)
    {
        BitStreamReader reader = args.CreateReader();
        return new IncomingArizonaPacket<TPayload>(PacketId.ArizonaCef, (int)subId, subId.ToString(), parser(ref reader));
    }

    private static OutgoingArizonaPacket<TPayload> ParseOutgoing220<TPayload>(OutgoingArizonaPacketArgs args, ArizonaPacketId subId, ArizonaReaderParser<TPayload> parser)
    {
        BitStreamReader reader = args.CreateReader();
        return new OutgoingArizonaPacket<TPayload>(PacketId.ArizonaCef, (int)subId, subId.ToString(), parser(ref reader));
    }

    private static IncomingArizonaPacket<TPayload> ParseIncoming221<TPayload>(IncomingArizonaPacketArgs args, ArizonaPacketIdEx subId, ArizonaReaderParser<TPayload> parser)
    {
        BitStreamReader reader = args.CreateReader();
        return new IncomingArizonaPacket<TPayload>(PacketId.ArizonaCefEx, (int)subId, subId.ToString(), parser(ref reader));
    }

    private static OutgoingArizonaPacket<TPayload> ParseOutgoing221<TPayload>(OutgoingArizonaPacketArgs args, ArizonaPacketIdEx subId, ArizonaReaderParser<TPayload> parser)
    {
        BitStreamReader reader = args.CreateReader();
        return new OutgoingArizonaPacket<TPayload>(PacketId.ArizonaCefEx, (int)subId, subId.ToString(), parser(ref reader));
    }
}
