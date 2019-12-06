.orga 0x1202400
	LW V1, 0x80361160
	LUI A0, 0x8040
	
	; Prepare address, assuming 0E at 80420000
	LBU T1, 0x21(V1)
	SLL T4, T1, 0x10
	ADDU A0, A0, T4
	
	LHU T2, 0x188(V1)
	ADDU A0, A0, T2
	
	; Prepare vertex count
	LHU A1, 0x18A(V1)
	BNEZ A1, noext
	LBU T3, 0x25(V1)
	ADD A1, T3, T3
	ADD A1, A1, T3
noext:

	; Prepare speed, must be multiple of 2
	LBU A2, 0x29(V1)
	
	; Count the UV loop
	LI T9, 0x1000
	LHU S0, 0x40(V1)
	SLT AT, S0, T9
	BNEZ AT, nouvfixup
	ADDU S0, S0, A2
	ADDU S0, R0, A2
	SUBI A2, A2, 0x1000
nouvfixup:
	SH S0, 0x40(V1)
	
	; Do it
loop:
	LHU T2, 0x0(A0)
	ADDU T2, T2, A2
	SH T2, 0x0(A0)
	SUBIU A1, A1, 1
	BNEZ A1, loop
	ADDIU A0, A0, 0x10

	JR RA
	NOP
	
	NOP
	NOP
	NOP
	NOP
	NOP
	NOP
	NOP
	NOP