// Felix has 4 evocations
Elemental Guardian - Summons an Elemental Guardian
Creation - Summons a random Rune
Elemental Drain - Inflicts 30% of total hp on every allied rune on the field
	to increment 60% dmg of the Elemental Guardian
Runic Overcharge - Increment Runic Power of 1 on a target Rune

// Rune
Passes turns automatically
On each start turn: heal the Elemental Guardian if distance <= 3; +1 Runic Power
When Runic Power reaches level 5, the Rune will explode:
	it damages every hero in 3 cells (explosion system is like bomb system)
