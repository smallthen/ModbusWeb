#!/usr/bin/env python3
import os,sys,time,random
try:
    from modbus_tk import modbus_tcp
except ImportError:
    print("请先执行: pip3 install modbus-tk")
    sys.exit(1)

server = modbus_tcp.TcpServer(port=502)
server.start()
slave = server.add_slave(1)
slave.add_block('0', 3, 0, 10)
print("✅ Modbus TCP Slave 已启动于 502，寄存器 0-9 每秒刷新")
while True:
    slave.set_values('0', 0, [random.randint(100, 200) for _ in range(10)])
    time.sleep(1)