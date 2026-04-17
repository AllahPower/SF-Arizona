using System.Numerics;

namespace SFSharp.Runtime.Networking;

// Shared helper payloads used by multiple Arizona packet models.
public readonly record struct ArzPlayerAnimGroupEntry(string GroupName, uint PackedValue, string AnimationName, byte Selector);
public readonly record struct ArzPlayerAnimGroupBatch(byte Opcode, ArzPlayerAnimGroupEntry[] Entries);

public abstract record ArzVehicleMaterialsOp;
public sealed record ArzVehicleMaterialsFieldPatchOp(byte Count, ArzVehicleMaterialsFieldPatch[] Patches) : ArzVehicleMaterialsOp;
public readonly record struct ArzVehicleMaterialsFieldPatch(byte FieldId, byte ValueKind, uint U32Value, byte U8Value);
public sealed record ArzVehicleMaterialsNamedToggleOp(string[] Names) : ArzVehicleMaterialsOp;
public sealed record ArzVehicleMaterialsNamedTransformOp(ArzVehicleMaterialsNamedTransform[] Entries) : ArzVehicleMaterialsOp;
public readonly record struct ArzVehicleMaterialsNamedTransform(string Name, Vector3 Delta, uint ExtraValue);
public sealed record ArzVehicleMaterialsResetByNameOp(string[] Names) : ArzVehicleMaterialsOp;
public sealed record ArzVehicleMaterialsNamedPairOp(ArzVehicleMaterialsNamedPair[] Entries) : ArzVehicleMaterialsOp;
public readonly record struct ArzVehicleMaterialsNamedPair(string Name, uint PackedValue, string OtherName);
public readonly record struct ArzVehicleMaterialsPacket(ushort VehicleId, ArzVehicleMaterialsOp[] Operations);

public readonly record struct ArzAttachVehicleToVehicleDataDescriptor(
    Vector3 Offset,
    Vector3 RotationDegrees,
    byte[] ComponentIds,
    byte FeatureFlags,
    byte VariantId,
    ushort ModelId,
    byte ExtraByte0,
    byte ExtraByte1,
    float DrawDistance);

public readonly record struct ArzNavigationArrowTarget(ushort X, ushort Y, ushort Z, ushort Radius);
public readonly record struct ArzVehicleDamageDoorPanelRule(ushort Key, float Value);

public abstract record ArzGpsRoutePoint;
public sealed record ArzGpsRouteWorldPoint(Vector3 Position) : ArzGpsRoutePoint;
public sealed record ArzGpsRouteLinePoint(byte LineType, ushort LineId, Vector3 Position) : ArzGpsRoutePoint;
public sealed record ArzGpsRoutePedBonePoint(bool UseEntitySpace, ushort EntityId, ushort BoneId) : ArzGpsRoutePoint;
public sealed record ArzGpsRouteVehiclePoint(ushort VehicleId, string Label, Vector3 Position) : ArzGpsRoutePoint;

#region outgoing (client -> server)
public readonly record struct ArzSendKey(byte Key, byte Unknown);
public readonly record struct ArzSendSwitchChatState(bool IsOpen);
public readonly record struct ArzSendTurnLights(byte State);
public readonly record struct ArzInjectCodeResponse(uint BrowserId, uint RequestId);
public readonly record struct ArzSendText(string Text, uint BrowserId);
public readonly record struct ArzModuleReadResponse(uint ModuleOffset, string ModuleName, byte Status, byte[] Data);
public readonly record struct ArzBrowserControlStateReply(uint BrowserId, bool State);
// vorbisFile.dll builds this from storage/device data and volume serial,
// XORs with the serial-derived key, then sends SHA-256 as lowercase hex bytes.
public readonly record struct ArzSendHWID(byte[] HexDigestBytes);
public readonly record struct ArzSendVehicleSpeedLimiterState(byte State);
public readonly record struct ArzSendSwitchChatMode(byte Mode);
public readonly record struct ArzSendSrcursorPosition(float X, float Y);
public readonly record struct ArzInCarNanCheckReport(byte ReportKind, ushort VehicleId);
public readonly record struct ArzSendKeyboardLayoutCapsState(byte KeyboardLayoutLowByte, bool CapsLockOn);
public readonly record struct ArzSendFloatValue(float Value);
public readonly record struct ArzSendToggleActionState(bool State);
public readonly record struct ArzSendTargetPosition(Vector3 Position);
public readonly record struct ArzSendSimpleTuningProgress(uint AccumulatedValue, byte TierStep);
public readonly record struct ArzSendCommandLine(string CommandLine);
public readonly record struct ArzSendDroneHeading(float Heading);
public readonly record struct ArzSendPortalToggle(byte State);
public readonly record struct ArzSendPortalPlacementPreview(byte PortalType, Vector3 PointA, Vector3 PointB);
public readonly record struct ArzSendWeaponScroll(byte Direction);
public readonly record struct ArzSendDamageResponseWeapon(byte WeaponId);
public readonly record struct ArzSendNavigationArrowSelection(byte SelectedIndex);

#endregion

#region incoming (server -> client)
public readonly record struct ArzSetLocalDriver(byte SeatCode, bool State);
public readonly record struct ArzTurnLightUpdate(ushort VehicleId, byte State);
public readonly record struct ArzSetSatiety(byte Satiety);
public readonly record struct ArzSetHudMode(byte Mode);
public readonly record struct ArzSetRadarMode(byte Mode);
public readonly record struct ArzPlayMediaOnBillboard(int BillboardId, byte[] Pad12A, string Link, string UserAgent, byte[] Pad12B);
public readonly record struct ArzClose(string BrowserId);
public readonly record struct ArzMove(string BrowserId, uint X, uint Y);
public readonly record struct ArzChangeUrl(string BrowserId, string Url);
public readonly record struct ArzInjectCode(string BrowserId, string Code, uint RequestId);
public readonly record struct ArzSendMessage(string MessageText, uint MessageId);
public readonly record struct ArzToggleScreen(string BrowserId);
public readonly record struct ArzToggleShow(string BrowserId);
public readonly record struct ArzBrowserClick(string BrowserId, uint X, uint Y, byte MouseButton);
public readonly record struct ArzGetBrowserControlState(string BrowserId);
public readonly record struct ArzSetBrowserControlState(string BrowserId, bool State);
public readonly record struct ArzResize(string BrowserId, uint Width, uint Height);
public readonly record struct ArzAddObject(string BrowserId, uint Value0, uint Value1);
public readonly record struct ArzRemoveObject(string BrowserId, uint Value0, uint Value1);
public readonly record struct ArzUiColorScale(ushort BrowserId, uint Argb, float Scale, ushort U16a, ushort U16b, byte Flags);
public readonly record struct ArzSetChatGroup(byte ChatId, string Icon, int Color, string ChatName, byte Flags)
{
    public bool IsVisible => (Flags & 1) != 0;
    public uint ArgbColor => ((uint)Color >> 8) | 0xFF000000;
}
public readonly record struct ArzHideDynamicRoom(byte RoomId);
public readonly record struct ArzSetLocalInVehicle(byte State);
public readonly record struct ArzSetNicknameMode(byte Mode);
public readonly record struct ArzSetChatFlag(byte State);
public readonly record struct ArzSwitchChatMode(byte Mode);
public readonly record struct ArzSrcursorSyncMode(byte Mode, float? MinCursorDelta);
public readonly record struct ArzTranslateObservedTextDrawPosition(ushort TextDrawId, float X, float Y);
public readonly record struct ArzWaypoint3DSetPosition(bool Enabled, Vector3? Position);
public readonly record struct ArzShowPositionInDiscord(bool Status);
public readonly record struct ArzChatCommandHelperEnabled(bool Enabled);
public readonly record struct ArzUnknown74(byte Value);
public readonly record struct ArzDiscordSetStateText(string Text);
public readonly record struct ArzDiscordClearStateText;
public readonly record struct ArzSetRadarVisibility(bool State);
public readonly record struct ArzSetCompassMode(byte Mode);
public readonly record struct ArzSetCompassCoords(float X, float Y);
public readonly record struct ArzShowStunIcon(byte PrimaryCounter, byte SecondaryCounter, byte TertiaryCounter);
public readonly record struct ArzHideStunIcon;
public readonly record struct ArzAutoDrinkBeer(bool State);
public readonly record struct ArzSetDayNightColors(bool NightMode);
public readonly record struct ArzToggleCompass(bool State);
public readonly record struct ArzSetAnimationProperty(uint Value);
public readonly record struct ArzToggleCgps(bool State);
public readonly record struct ArzToggleMapColors(bool State);
public readonly record struct ArzSetRenderRoutineEnabled(bool Enabled);
public readonly record struct ArzChangeServer(string Host, uint Port, string Nickname, string Password, bool ConnectMode);
public readonly record struct ArzShowLoadScreenVc(byte BgType, uint? Timeout);
public readonly record struct ArzSetVehicleFlightForwardAssist(bool Enabled);
public readonly record struct ArzSetChatIconState(uint PlayerId, bool Active);
public readonly record struct ArzSetGreenZone(byte Mode);
public readonly record struct ArzSetVehicleModelSpeedLimit(float SpeedLimitOrMinusOne, ushort[] VehicleModels);
public readonly record struct ArzSetSpectatorPatches(byte State, byte Unknown);
public readonly record struct ArzSetActionStateToggleEnabled(bool Enabled);
public readonly record struct ArzSetViceCityFlag(bool State);
public readonly record struct ArzSetTuningConfig(byte Value);
public readonly record struct SetPlayerNametagFlags(ushort Id, byte[] RawPayload, bool? TrailingBit);
public readonly record struct ArzLoadSharedTexture(byte[] Data);
public readonly record struct ArzToggleSharedTxdFlag(bool State);
public readonly record struct ArzStreamFixMode(byte Mode);
public readonly record struct ArzSetMapIcon(byte IconId, byte[] Pad14, ushort IconModel, Vector3 Position, string IconName, byte Pad);
public readonly record struct ArzDeleteCustomMarker(uint MarkerId);
public readonly record struct ArzClearCustomMarkers;
public readonly record struct ArzTestDrive(ushort VehicleId, bool State);
public readonly record struct ArzSetVehicleLightsColor(ushort VehicleId, uint Argb);
public readonly record struct ArzUiScalar(ushort BrowserId, byte Index, float Value);
public readonly record struct ArzSetDriveOnWater(bool State);
public readonly record struct ArzSetVehicleFlight(bool State);
public readonly record struct ArzAttachVehicleToVehicleData(
    ushort VehicleId,
    byte Slot,
    bool HasData,
    ArzAttachVehicleToVehicleDataDescriptor? Data);
public readonly record struct ArzSetVehicleColorSmoke(ushort VehicleId, float Intensity, byte R, byte G, byte B);
public readonly record struct ArzCreate3DWaypoint(ushort PlayerId, uint Color, float X, float Y, uint Timeout, uint Extra, bool Active);
public readonly record struct ArzSetVehicleNeonColor(ushort VehicleId, byte R, byte G, byte B, byte A);
public readonly record struct ArzSetSkyboxImages(byte Tag0, byte Tag1, byte Tag2, string Names, uint Offset1, uint Offset2, uint Offset3, uint Offset4, uint Offset5, ushort End);
public readonly record struct ArzSetHudStyle(byte Style);
public readonly record struct ArzToggleRenderTarget(bool Create);
public readonly record struct ArzVehicleFeatureFlag1(ushort VehicleId, bool State);
public readonly record struct ArzVehicleFeatureFlag0(ushort VehicleId, bool State);
public readonly record struct ArzVehicleFeatureFlag2(ushort VehicleId, bool State);
public readonly record struct ArzSetVehicleNumberPlate(ushort VehicleId, byte PlateType, string PlateText, string PlateRegion);
public readonly record struct ArzSetPlayerAttachedObject(ushort PlayerId, int Index, bool Create, int Bone, int ModelId, Vector3 Offset, Vector3 Rotation, Vector3 Scale, int Color1, int Color2);
public readonly record struct ArzVehicleFeatureReset(ushort VehicleId, bool State);
public readonly record struct ArzSetWeaponUpgrade(byte WeaponId, byte[] RawPayload);
public readonly record struct ArzSetPlayerAnimGroups(ushort PlayerId, ArzPlayerAnimGroupBatch[] Batches);
public readonly record struct ArzLoadBinary(string Text);
public readonly record struct ArzSetSelectorHookEnabled(bool Enabled);
public readonly record struct ArzSetSelectorSlotBlocked(bool Blocked);
public readonly record struct ArzTogglePortal(bool State);
public readonly record struct ArzCreatePortal(ushort Id, byte Type, Vector3 Position, Vector3 Rotation);
public readonly record struct ArzDestroyPortal(ushort Id, byte Type);
public readonly record struct ArzSetSingleAnimGroup(ushort PlayerId, string GroupName);
public readonly record struct ArzSetCurrentTask(byte Unused, string Text, string Emoji);
public readonly record struct ArzToggleDrawInterface(bool Status);
public readonly record struct ArzSetInterior(Vector3 Position, ushort Pad, byte Interior, byte[] Remaining);
public readonly record struct ArzUiToggle(ushort BrowserId, bool State);
public readonly record struct ArzSetWaterLevel(byte Mode, float Level, float? TargetLevel, uint? DurationMs);
public readonly record struct ArzWallHackToggle(bool Enabled);
public readonly record struct ArzVehicleHeadlightsState(ushort VehicleId, bool State);
public readonly record struct ArzSetVirtualWorld(uint World);
public readonly record struct ArzSetVehicleDriftMode(ushort VehicleId, bool State);
public readonly record struct ArzSetLines(byte Action, ushort LineId, byte[] RawPayload);
public readonly record struct ArzRadarFixPlayerStyle(ushort PlayerIndex, byte? Style, bool? LockFlag);
public readonly record struct ArzSetVehicleLights(ushort VehicleId, string LightName);
public readonly record struct ArzSimpleAttachmentsSetMaterial(ushort PlayerId, ushort AttachIndex, byte Selector, string MaterialName, string TextureName, byte Byte0, byte Byte1, byte Byte2, byte Byte3);
public readonly record struct ArzResetFirstPersonState;
public readonly record struct ArzUpdateWeaponSlots(byte WeaponId, byte[] RawPayload);
public readonly record struct ArzNavigationArrowTargets(bool FollowVertical, bool SpecialMode, ArzNavigationArrowTarget[] Targets);
public readonly record struct ArzUpdateQueuePosition(byte Mode, ushort? QueuePosition, byte? Extra)
{
    public bool HasQueuePosition => Mode == 0 && QueuePosition.HasValue;
}
public readonly record struct ArzUnknown200(byte Mode);
public readonly record struct ArzGoogleAnalyticsMessage(string Text, uint Flags);
public readonly record struct ArzSetVehicleStrobelights(ushort VehicleId, byte Step, float Speed, bool Beam);
public readonly record struct ArzChatMessageRelay(uint ColorRgba, byte ChatType, byte[] RawPayload)
{
    public uint ArgbColor => (ColorRgba >> 8) | 0xFF000000;
}
public readonly record struct ArzAttachVehicleToVehicleToggle(bool Enabled);
public readonly record struct ArzSetGpsRoute(byte Action, byte Slot, byte Speed, bool Loop, int Color1, int Color2, ArzGpsRoutePoint? First, ArzGpsRoutePoint? Second)
{
    public bool IsCreate => Action == 1;
    public bool IsRemove => Action == 0;
}
public readonly record struct ArzVehicleDamageDoorPanelRules(ushort GroupId, ArzVehicleDamageDoorPanelRule[] Entries);
public readonly record struct ArzSetFirstPersonCamera(bool State);
public readonly record struct ArzSetExtendAnimGroups(ushort PlayerId, string GroupName);
public readonly record struct ArzDirtySampObjectsMakeObjectDirty(bool IsAttachedObject, byte DirtyLevel, ushort ObjectId, byte? AttachIndex, byte? Extra);
public readonly record struct ArzSetVehicleBrakeCalipersModel(ushort VehicleId, bool Toggle, bool? IsSimpleModel, ushort? ModelId);
public readonly record struct ArzToggleHeadMove(bool State);
public readonly record struct ArzSetVehicleBrakeCalipers(ushort VehicleId, byte Count, ushort[] ModelIds, uint? ExtraParam);

#endregion

#region multiplexed / aliased packet IDs
public readonly record struct ArzLoadJs(byte[] Unknown16, string Js, string Any, uint BrowserId);
public readonly record struct ArzSimpleCreate(uint Width, uint Height, uint X, uint Y, string PrimaryText, string SecondaryText, uint? ExtraInt, float? ExtraFloat);
public readonly record struct ArzCreateScaled(uint Width, uint Height, uint X, uint Y, string PrimaryText, string SecondaryText, float Scale, uint? ExtraInt, float? ExtraFloat);
public readonly record struct ArzObjectCreate(uint Width, uint Height, uint X, uint Y, string PrimaryText, string SecondaryText, ushort Short0, ushort Short1, float FloatValue, uint? ExtraInt, float? ExtraFloat);
public readonly record struct ArzInsideObjectCreate(uint Width, uint Height, uint X, uint Y, string PrimaryText, string SecondaryText, ushort Short0, ushort Short1, float FloatValue, uint Value4, uint Value5, uint? ExtraInt, float? ExtraFloat);
public readonly record struct ArzRequestClientViewport;
public readonly record struct ArzStatePair(uint Width, uint Height);
public readonly record struct ArzModuleReadRequest(uint ModuleOffset, string ModuleName, uint Size);
public readonly record struct ArzSetVisibleDistance3DMarker(bool Status, float Distance, byte Pad);
public readonly record struct ArzUiConfig(byte Type, byte Len);
public readonly record struct ArzScaleRadarMapIcon(byte RadarIconId, float ScaleX, float ScaleY);
public readonly record struct ArzGangZonePoly(byte ZoneId, uint[] PackedPolygonPoints, byte ColorR, byte ColorG, byte ColorB, byte ColorA, byte Style, bool Enabled);

#endregion
