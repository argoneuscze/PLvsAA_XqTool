import os
import sys

filename = sys.argv[1]

with open(filename, 'r', encoding='Shift-JIS') as f:
    lines = [x.strip() for x in f.readlines()]

result = dict()

for i in range(0, len(lines), 2):
    first = lines[i]
    second = lines[i + 1]

    charname = first#.split(' ')[5]
    try:
        emote = second#.split(' ')[5]
    except IndexError:
        continue

    if result.get(charname) is None:
        result[charname] = set()

    result[charname].add(emote)

target_path = os.path.splitext(filename)[0]

try:
    os.makedirs(target_path)
except FileExistsError:
    pass

for key in result.keys():
    file = open('{}/{}.txt'.format(target_path, key), 'w', encoding='utf-8')

    file.write('<T><R0.3><W300>\n')
    for item in sorted(result[key]):
        file.write('<MS"{}"><W200>'.format(item))
        file.write(150 * ' ')
        file.write('<W1000>\n')

    file.write('</R>')

    file.close()
