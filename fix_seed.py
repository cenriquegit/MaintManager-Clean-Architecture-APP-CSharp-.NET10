import re
with open(r'C:\Users\carlo\Desktop\proyect\05_seed_final.sql', 'r', encoding='utf-8') as f:
    content = f.read()

# Replace , 2, with , 'juan.quispe', in maintenance.ins_main calls
# Replace , 3, with , 'pedro.mamani', in maintenance.ins_main calls
lines = content.split('\n')
result = []
for line in lines:
    if 'maintenance.ins_main' in line:
        parts = line.split(',')
        if len(parts) >= 8:
            p8 = parts[7].strip()
            if p8 == '2':
                parts[7] = " 'juan.quispe'"
                line = ','.join(parts)
            elif p8 == '3':
                parts[7] = " 'pedro.mamani'"
                line = ','.join(parts)
    result.append(line)

output = '\n'.join(result)

# Verify no remaining hardcoded workid
remaining = re.findall(r', \d+,.*assigned_username|ins_main.*?, \d+,', output)
print(f"Hardcoded workid remaining: {len(remaining)}")

with open(r'C:\Users\carlo\Desktop\proyect\05_seed_final.sql', 'w', encoding='utf-8') as f:
    f.write(output)

print("Done")
