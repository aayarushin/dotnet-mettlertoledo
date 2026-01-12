# Channel-Specific Behavior Testing



## Channel mocking

Created **three distinct mock implementations**, each serving a different testing purpose:

### 1. **MockChannel** (Generic Protocol Testing)
**Purpose:** Fast, simple protocol validation
-  Generic SICS protocol testing
-  Response configuration
-  No channel-specific behavior
-  Fastest execution (~10ms fixed)

### 2. **MockEthernetChannel** (TCP/IP Behavior)
**Purpose:** Emulate network characteristics
-  **Network latency** simulation (configurable 5-50ms+)
-  **TCP packet-based** transmission (1 packet per message)
-  **Packet fragmentation** for large messages
-  **Connection-oriented** (connect/disconnect)
-  Fast initialization (~10ms)
-  Simulates round-trip time

### 3. **MockSerialChannel** (RS-232 Behavior)
**Purpose:** Emulate serial port characteristics
-  **Baud rate affects speed** (1200-115200)
-  **Byte-by-byte** transmission simulation
-  **Serial buffer** management (4KB limit)
-  **Buffer overflow** scenarios
-  Slower initialization (~50ms hardware setup)
-  Byte transmission delay calculation

## Key Behavioral Differences Tested

| Characteristic | Ethernet (MockEthernetChannel) | Serial (MockSerialChannel) |
|----------------|-------------------------------|---------------------------|
| **Initialization Time** | ~10ms (TCP handshake) | ~50ms (hardware setup) |
| **Latency Source** | Network round-trip | Baud rate calculation |
| **Transmission Model** | Packet-based (1 packet) | Byte stream (N bytes) |
| **PacketsReceived** | 1 (or fragmented) | = BytesReceived |
| **Fragmentation** | ? Yes (configurable) | ? No (continuous stream) |
| **Buffer** | Large TCP buffer | Limited (4KB) |
| **Speed Scaling** | Network latency config | 10 bits/byte at baud rate |
| **Typical Duration** | 10-50ms | Varies by baud (1ms-500ms) |
