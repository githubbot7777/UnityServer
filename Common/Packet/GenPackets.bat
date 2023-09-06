START ../../PacketGenerator/bin/PacketGenerator.exe ../../PacketGenerator/PDL.xml
XCOPY /Y GenPackets.cs "../../Client/Packet"
XCOPY /Y GenPackets.cs "../../Server/Packet"
XCOPY /Y ClientPacketManager.cs "../../Client/Packet"
XCOPY /Y ServerPacketManager.cs "../../Server/Packet"