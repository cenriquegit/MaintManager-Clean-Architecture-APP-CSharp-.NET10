with open(r'C:\Users\carlo\Desktop\proyect\05_seed_final.sql', 'r') as f:
    content = f.read()

# Replace all patterns where ', 2,' appears as the 8th param in function calls
# Since calls are multi-line, just do a global replace of ', 2,' with ", 'juan.quispe'," but only in the context of ins_main calls
# Actually simpler: just replace ', 2,' -> ", 'juan.quispe'," and ', 3,' -> ", 'pedro.mamani',"
# ONLY in lines between 'ins_main' calls. Let's use a flag.

lines = content.split('\n')
result = []
in_ins_main = False
for line in lines:
    # Check if we're inside an ins_main call
    if 'ins_main(' in line and 'SELECT' in line:
        in_ins_main = True
    # Check if the line ends the call
    if in_ins_main and "');" in line:
        in_ins_main = False
    
    if in_ins_main:
        # Replace the 8th parameter which is ', 2,' or ', 3,' on its own line
        # These are the continuation lines that look like "    'text', 'text', 2,"
        # or "    3,"
        # We need to be careful: only replace when 2 or 3 is the assigned_to param
        # The assigned_to is the 8th param, which appears after 7 commas on the SELECT line
        # On continuation lines, it's harder to know which param position we're at.
        
        # Simple approach: just replace ', 2,' and ', 3,' in the ins_main block
        # The risk is low since only ins_main calls use these values.
        if ', 2,' in line:
            line = line.replace(', 2,', ", 'juan.quispe',")
        if ', 3,' in line:
            line = line.replace(', 3,', ", 'pedro.mamani',")
    
    result.append(line)

output = '\n'.join(result)
count_2 = output.count(', 2,')
count_3 = output.count(', 3,')
print(f"Remaining ', 2,': {count_2}")
print(f"Remaining ', 3,': {count_3}")

with open(r'C:\Users\carlo\Desktop\proyect\05_seed_final.sql', 'w') as f:
    f.write(output)
print("Done")
