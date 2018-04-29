import zlib

with open("opcode_strings.txt", "r") as file:
    content = file.readlines()

values = dict()

lines = [line.strip() for line in content]
for line in lines:
    b = line.encode("Shift-JIS")
    crc = zlib.crc32(b) & 0xffffffff
    values[crc] = line

strings = []

for value in values.items():
    strings.append('{{{}, "{}"}}'.format(value[0], value[1]))

print(',\n'.join(strings))
