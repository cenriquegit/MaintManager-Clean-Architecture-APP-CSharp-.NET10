with open(r'C:\Users\carlo\Desktop\proyect\05_seed_final.sql', 'r') as f:
    content = f.read()

# Replace all the corrupted function calls manually
fixes = {
    # Line 573: wrong: 1, 'juan.quispe'  should be: 1, 2
    "SELECT maintenance.ins_main('VDW-869', 1, 'juan.quispe', 'ORD-2024-VDW869-B',":
     "SELECT maintenance.ins_main('VDW-869', 1, 2, 'ORD-2024-VDW869-B',",
    
    # Line 589: wrong: 1, 'juan.quispe'  should be: 1, 2
    "SELECT maintenance.ins_main('VBQ-302', 1, 'juan.quispe', 'ORD-2024-VBQ302-B',":
     "SELECT maintenance.ins_main('VBQ-302', 1, 2, 'ORD-2024-VBQ302-B',",
    
    # Line 600: wrong: 1, 'juan.quispe'  should be: 1, 2
    "SELECT maintenance.ins_main('VBQ-285', 1, 'juan.quispe', 'ORD-2024-VBQ285-B',":
     "SELECT maintenance.ins_main('VBQ-285', 1, 2, 'ORD-2024-VBQ285-B',",
    
    # Line 617: wrong: 1, 'juan.quispe'  should be: 1, 2
    "SELECT maintenance.ins_main('V0U-053', 1, 'juan.quispe', 'ORD-2024-V0U053',":
     "SELECT maintenance.ins_main('V0U-053', 1, 2, 'ORD-2024-V0U053',",
    
    # Line 627: wrong: 1, 'juan.quispe'  should be: 1, 2
    "SELECT maintenance.ins_main('V0U-053', 1, 'juan.quispe', 'ORD-2025-V0U053',":
     "SELECT maintenance.ins_main('V0U-053', 1, 2, 'ORD-2025-V0U053',",
    
    # Line 638: wrong: 1, 'juan.quispe'  should be: 1, 2
    "SELECT maintenance.ins_main('VAK-826', 1, 'juan.quispe', 'ORD-2024-VAK826-B',":
     "SELECT maintenance.ins_main('VAK-826', 1, 2, 'ORD-2024-VAK826-B',",
    
    # Line 643: wrong: 'juan.quispe', NULL  should be: 2, NULL
    "SELECT maintenance.ins_main('VAK-826', 'juan.quispe', NULL, 'ORD-2025-VAK826-EME',":
     "SELECT maintenance.ins_main('VAK-826', 2, NULL, 'ORD-2025-VAK826-EME',",
    
    # Line 654: wrong: 1, 'juan.quispe'  should be: 1, 2
    "SELECT maintenance.ins_main('B0J-433', 1, 'juan.quispe', 'ORD-2025-B0J433',":
     "SELECT maintenance.ins_main('B0J-433', 1, 2, 'ORD-2025-B0J433',",
}

for wrong, correct in fixes.items():
    content = content.replace(wrong, correct)

# Verify no corrupted calls remain
if any(x in content for x in fixes.keys()):
    print("WARNING: Some fixes were NOT applied!")
else:
    print("All fixes applied successfully.")

with open(r'C:\Users\carlo\Desktop\proyect\05_seed_final.sql', 'w') as f:
    f.write(content)
print("Done")
