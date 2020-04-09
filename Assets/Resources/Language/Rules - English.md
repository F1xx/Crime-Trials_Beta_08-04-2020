RULES:

For files:

1. All language files must end in .txt to be parsed.
2. They must exist in the Resources/Language folder to be parsed.
3. It's recommended you use Notepad++ for editing language files and set the Lanauge to Markdown or C for better
syntax highlighting.

For data structures (AudioWrappers):

1. All data structures MUST BE defined in brackets with a unique name. 
		Example: [VO_Daryl_Greeting_1]
2. All data structures MUST BE undefined in brackets with forward slashes with their name. 
		Example: [/VO_Daryl_Greeting_1/]
3. Any line inside a data structure with content must follow a float:string format. 
		Example: 3.1:Daryl: Hello there partner!
4. Any line with content outside a data structure will be ignored by the parser.
5. Comments will be completely ignored. They can be denoted with // or /**/ format.
6. The name of the data structure here must match the name of their AudioWrapper inside Unity.
7. If there are multiple definitions for a single data structure, the last structure is the one that will retain.


Example of a clean data structure with multiple subtitle lines:

[VO_Darwin_Quest_Epilogue]
4.1:Darwin: It's finally over... this horrible nightmare.
3.3:Darwin: I honestly thought it would never end
4.3:Darwin: when the demons broke out of Highmountain.
5.2:Darwin: You deserve a reward for your efforts, hero.
5.3:Darwin: Choose something you like from my private stock.
[/VO_Darwin_Quest_Epilogue/]




Example of a data structure that uses white space and comments (still perfectly legal script):

[VO_Darwin_Quest_Start]

//First the quest NPC gets the hero's attention
2.2:Darwin: Hero, wait!

/*
	In this moment, Darwin explains his plight to the hero while the player can hear
	demons banging on doors in the background. The player can tell that this NPC
	is probably not lying and be inclined to help.
*/
5.5:Darwin: There's rumors of demons in Highmountain...
4.4:Darwin: they're trying to escape! 
5.0:Darwin: I need you to stop them before it's all over!

//Exit text
6.5:Darwin: Please... you're my only hope... hero!

[/VO_Darwin_Quest_Start/]