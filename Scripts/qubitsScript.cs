using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class qubitsScript : MonoBehaviour {

	public KMBombInfo bomb;
	public KMAudio audio;

	static int moduleIdCounter = 1;
	int moduleId;
	bool moduleSolved;

	public KMSelectable display;

	public KMSelectable[] buttons;

	bool bit1;
	bool bit2;
	bool bit3;
	bool bit4;
	bool bit5;
	bool bit6;

	bool[] favors;


	int entangledPos1;
	int entangledPos2;
	bool directEntang;

	int numPressed;


	bool[] isPressed = {false, false, false, false, false, false};

	bool strike;

	int startingValue;
	int leftEnt;
	int rightEnt;
	int[] values = {0,0,0,0,0,0};
	string[] answer = {"-1", "-1", "-1", "-1", "-1", "-1"};
	string[] buttonText = {"", "", "", "", "", ""};
	int finalValue;



	void Awake ()
	{
		moduleId = moduleIdCounter++;
		
		display.OnHighlight += OnDisplayHover;
		display.OnHighlightEnded += OffDisplayHover;
		display.OnInteract += delegate () {displayPress(); return false;};

		for(int i = 0; i < 6; i++)
		{
			int index = i;
			buttons[i].OnHighlight += delegate () {OnButtonHover(index);};
			buttons[i].OnHighlightEnded += delegate () {OffButtonHover(index);};
			buttons[i].OnInteract += delegate () {ButtonPress(index); return false; };
		}

		GeneratePuzzle();
	}

	void GeneratePuzzle()
	{
		entangledPos1 = Random.Range(0, 6);
		entangledPos2 = Random.Range(0, 6);

		

		while (entangledPos1 == entangledPos2)
		{
    		entangledPos2 = Random.Range(0, 6);
		}

		directEntang = Random.value > 0.5f;
		string state;

		if(directEntang){
			state = "directly";
		} else
		{
			state = "inversely";
		}

		Debug.LogFormat("[Qubits] #{0} The top qubits {1} and {2} are {3} entangled", moduleId, entangledPos1 + 1, entangledPos2 + 1, state);

		bit1 = Random.value > 0.5f;
		bit2 = Random.value > 0.5f;
		bit3 = Random.value > 0.5f;
		bit4 = Random.value > 0.5f;
		bit5 = Random.value > 0.5f;
		bit6 = Random.value > 0.5f;

        favors = new bool[] { bit1, bit2, bit3, bit4, bit5, bit6 };
		string[] favorString = {"0", "0", "0", "0", "0", "0"};
		for (int i = 0; i < 6; i++)
		{
			if(favors[i])
			{
				favorString[i] = "1";
			} else
			{
				favorString[i] = "0";
			}

		}

		string joined = string.Join(", ", favorString);
		
		Debug.LogFormat(" ");
		Debug.LogFormat("The bottom qubits from left to right favor: {0}", joined);
		Debug.LogFormat(" "); 

    
	

	}


	void GenerateSolution()
	{
		int entVal1 = (int)Mathf.Pow(2, 5 - entangledPos1);
		int entVal2 = (int)Mathf.Pow(2, 5 - entangledPos2);

	
		int step2val;

		

		if(entangledPos1 > entangledPos2)
		{
			leftEnt = entangledPos2;
			rightEnt = entangledPos1;
		} else
		{
			rightEnt = entangledPos2;
			leftEnt = entangledPos1;
		}
		
		if(directEntang)
		{
			if(char.IsDigit(bomb.GetSerialNumber()[leftEnt]))
			{
				step2val = bomb.GetSerialNumber()[leftEnt] - '0';
			}
			else
			{
				step2val = char.ToLower(bomb.GetSerialNumber()[leftEnt]) - 'a' + 1;
			}
			if(bomb.GetPortCount() != 0){
				startingValue = (entVal1 + entVal2 + step2val) / bomb.GetPortCount();
			} else
			{
				startingValue = entVal1 + entVal2 + step2val;
			}
			
		} else
		{
			if(char.IsDigit(bomb.GetSerialNumber()[rightEnt]))
			{
				step2val = bomb.GetSerialNumber()[rightEnt] - '0';
			}
			else
			{
				step2val = char.ToLower(bomb.GetSerialNumber()[rightEnt]) - 'a' + 1;
			}
			if(bomb.GetBatteryCount() != 0){
				startingValue = ((entVal1 + entVal2) / 2 + step2val) / bomb.GetBatteryCount();
			} else
			{
				startingValue = (entVal1 + entVal2) / 2 + step2val;
			}
			
		}

		Debug.LogFormat("Starting value is {0}", startingValue);

		int favor0Count = 0;

		for (int i = 0; i < 6; i++)
		{
			if(!favors[i])
			{
				favor0Count++;
			}
		}

		for(int i = 0; i < 6; i++)
		{
			int currentValue = 0;
			if(favors[i])
			{
				currentValue += bomb.GetSerialNumberNumbers().Max();
			} else
			{
				currentValue += i + 1;
			}
			if(i == 1 || i == 2 || i == 4)
			{
				currentValue = currentValue * 7;
			} else
			{
				currentValue = currentValue * 4;
			}
			if(i == entangledPos1 || i == entangledPos2)
			{
				currentValue += bomb.GetBatteryHolderCount() + favor0Count;
			} else
			{
				currentValue -= bomb.GetPortPlateCount() + leftEnt + 1;
			}
			if( new[] {'Q', '6', 'B', '1', 'T', 'S', 'H', '8', 'R', 'M', 'Y', '3' ,'4', 'D'}.Contains(bomb.GetSerialNumber()[i]))
			{
				currentValue += bomb.GetIndicators().ToArray().Length + i + 1;
			} else
			{
				currentValue -= bomb.GetPortCount() + rightEnt + 1;
            }

            values[i] = currentValue;
		}
		Debug.LogFormat(" ");
		Debug.LogFormat("1st qubit:");
		Debug.LogFormat("Condition 1 is {0}", favors[0]);
		Debug.LogFormat("Condition 2 is False");
		Debug.LogFormat("Condition 3 is {0}", 0 == entangledPos1 || 0 == entangledPos2);
		Debug.LogFormat("Condition 4 is {0}", new[] {'Q', '6', 'B', '1', 'T', 'S', 'H', '8', 'R', 'M', 'Y', '3' ,'4', 'D'}.Contains(bomb.GetSerialNumber()[0]));
		Debug.LogFormat("The value of the 1st qubit is {0}", values[0]);
		Debug.LogFormat(" ");
		Debug.LogFormat("2nd qubit:");
		Debug.LogFormat("Condition 1 is {0}", favors[1]);
		Debug.LogFormat("Condition 2 is True");
		Debug.LogFormat("Condition 3 is {0}", 1 == entangledPos1 || 1 == entangledPos2);
		Debug.LogFormat("Condition 4 is {0}", new[] {'Q', '6', 'B', '1', 'T', 'S', 'H', '8', 'R', 'M', 'Y', '3' ,'4', 'D'}.Contains(bomb.GetSerialNumber()[1]));
		Debug.LogFormat("The value of the 2nd qubit is {0}", values[1]);
		Debug.LogFormat(" ");
		Debug.LogFormat("3rd qubit:");
		Debug.LogFormat("Condition 1 is {0}", favors[2]);
		Debug.LogFormat("Condition 2 is True");
		Debug.LogFormat("Condition 3 is {0}", 2 == entangledPos1 || 2 == entangledPos2);
		Debug.LogFormat("Condition 4 is {0}", new[] {'Q', '6', 'B', '1', 'T', 'S', 'H', '8', 'R', 'M', 'Y', '3' ,'4', 'D'}.Contains(bomb.GetSerialNumber()[2]));
		Debug.LogFormat("The value of the 3rd qubit is {0}", values[2]);
		Debug.LogFormat(" ");
		Debug.LogFormat("4th qubit:");
		Debug.LogFormat("Condition 1 is {0}", favors[3]);
		Debug.LogFormat("Condition 2 is False");
		Debug.LogFormat("Condition 3 is {0}", 3 == entangledPos1 || 3 == entangledPos2);
		Debug.LogFormat("Condition 4 is {0}", new[] {'Q', '6', 'B', '1', 'T', 'S', 'H', '8', 'R', 'M', 'Y', '3' ,'4', 'D'}.Contains(bomb.GetSerialNumber()[3]));
		Debug.LogFormat("The value of the 4th qubit is {0}", values[3]);
		Debug.LogFormat(" ");
		Debug.LogFormat("5th qubit:");
		Debug.LogFormat("Condition 1 is {0}", favors[4]);
		Debug.LogFormat("Condition 2 is True");
		Debug.LogFormat("Condition 3 is {0}", 4 == entangledPos1 || 4 == entangledPos2);
		Debug.LogFormat("Condition 4 is {0}", new[] {'Q', '6', 'B', '1', 'T', 'S', 'H', '8', 'R', 'M', 'Y', '3' ,'4', 'D'}.Contains(bomb.GetSerialNumber()[4]));
		Debug.LogFormat("The value of the 5th qubit is {0}", values[4]);
		Debug.LogFormat(" ");
		Debug.LogFormat("6th qubit:");
		Debug.LogFormat("Condition 1 is {0}", favors[5]);
		Debug.LogFormat("Condition 2 is False");
		Debug.LogFormat("Condition 3 is {0}", 5 == entangledPos1 || 5 == entangledPos2);
		Debug.LogFormat("Condition 4 is {0}", new[] {'Q', '6', 'B', '1', 'T', 'S', 'H', '8', 'R', 'M', 'Y', '3' ,'4', 'D'}.Contains(bomb.GetSerialNumber()[5]));
		Debug.LogFormat("The value of the 6th qubit is {0}", values[5]);
		
		
		

		finalValue = (values.Sum() + startingValue) % 64;

		while(finalValue < 0)
		{
			finalValue += 64;
		}

		Debug.LogFormat("Final sum mod 64 is {0}", finalValue);

	

		//turn into binary string array
		
		
		if(finalValue >= 32)
		{
			answer[0] = "1";
		} else
		{
			answer[0] = "0";
		}

		if(finalValue % 32 >= 16)
		{
			answer[1] = "1";
		} else
		{
			answer[1] = "0";
		}

		if(finalValue % 16 >= 8)
		{
			answer[2] = "1";
		} else
		{
			answer[2] = "0";
		}

		if(finalValue % 8 >= 4)
		{
			answer[3] = "1";
		} else
		{
			answer[3] = "0";
		}

		if(finalValue % 4 >= 2)
		{
			answer[4] = "1";
		} else
		{
			answer[4] = "0";
		}

		if(finalValue % 2 >= 1)
		{
			answer[5] = "1";
		} else
		{
			answer[5] = "0";
		}


		string answerString = string.Join("", answer);
		Debug.LogFormat("The correct answer is {0} in binary which is {1}", finalValue, answerString);


	}

	void Check()
	{
		numPressed = 0;
		if(buttonText.SequenceEqual(answer))
		{
			GetComponent<KMBombModule>().HandlePass();
			moduleSolved = true;
			buttons[0].GetComponentInChildren<TextMesh>().text = "S";
			buttons[1].GetComponentInChildren<TextMesh>().text = "O";
			buttons[2].GetComponentInChildren<TextMesh>().text = "L";
			buttons[3].GetComponentInChildren<TextMesh>().text = "V";
			buttons[4].GetComponentInChildren<TextMesh>().text = "E";
			buttons[5].GetComponentInChildren<TextMesh>().text = "D";
			display.GetComponentInChildren<TextMesh>().text = ":-D";

		} else{
			
			if(!moduleSolved) {

				GetComponent<KMBombModule>().HandleStrike();

				strike = true;

				isPressed[0] = false;
				isPressed[1] = false;
				isPressed[2] = false;
				isPressed[3] = false;
				isPressed[4] = false;
				isPressed[5] = false;

				buttons[0].GetComponentInChildren<TextMesh>().text = "";
				buttons[1].GetComponentInChildren<TextMesh>().text = "";
				buttons[2].GetComponentInChildren<TextMesh>().text = "";
				buttons[3].GetComponentInChildren<TextMesh>().text = "";
				buttons[4].GetComponentInChildren<TextMesh>().text = "";
				buttons[5].GetComponentInChildren<TextMesh>().text = "";
			}
		}
	}

	

	void OnDisplayHover()
	{	
		if(!moduleSolved){
			int disp1 = Random.Range(0, 2);
			int disp2 = Random.Range(0, 2);
			int disp3 = Random.Range(0, 2);
			int disp4 = Random.Range(0, 2);
			int disp5 = Random.Range(0, 2);
			int disp6 = Random.Range(0, 2);

			string disp1String = disp1.ToString();
			string disp2String = disp2.ToString();
			string disp3String = disp3.ToString();
			string disp4String = disp4.ToString();
			string disp5String = disp5.ToString();
			string disp6String = disp6.ToString();

			string[] disp = {disp1String, disp2String, disp3String, disp4String, disp5String, disp6String};

			
			if(directEntang)
			{
				disp[entangledPos2] = disp[entangledPos1];
			}
			else
			{
				if(disp[entangledPos1] == "1")
				{
					disp[entangledPos2] = "0";
				}
				else
				{
					disp[entangledPos2] = "1";
				}
			}

			string displayText = string.Join("", disp);
			display.GetComponentInChildren<TextMesh>().text = displayText;
		}
		
		
	}

	

	void OffDisplayHover()
	{
		if(!moduleSolved) {
			display.GetComponentInChildren<TextMesh>().text = "";
		}
	}

	void displayPress()
	{
		numPressed = 0;
		
		if(!moduleSolved) {
			isPressed[0] = false;
			isPressed[1] = false;
			isPressed[2] = false;
			isPressed[3] = false;
			isPressed[4] = false;
			isPressed[5] = false;

			buttons[0].GetComponentInChildren<TextMesh>().text = "";
			buttons[1].GetComponentInChildren<TextMesh>().text = "";
			buttons[2].GetComponentInChildren<TextMesh>().text = "";
			buttons[3].GetComponentInChildren<TextMesh>().text = "";
			buttons[4].GetComponentInChildren<TextMesh>().text = "";
			buttons[5].GetComponentInChildren<TextMesh>().text = "";

			
		}

		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		display.AddInteractionPunch();
	}

	void Observe(int i)
	{
		if(!isPressed[i] && !moduleSolved){
			if(favors[i])
			{
				bool bitShows1 = Random.value > 0.25f;
				if(bitShows1)
				{
					buttons[i].GetComponentInChildren<TextMesh>().text = "1";
				} else {
					buttons[i].GetComponentInChildren<TextMesh>().text = "0";
				}
				
			} else
			{
				bool bitShows1 = Random.value < 0.25f;
				if(bitShows1)
				{
					buttons[i].GetComponentInChildren<TextMesh>().text = "1";
				} else {
					buttons[i].GetComponentInChildren<TextMesh>().text = "0";
				}
			}
		}
	}

	void OnButtonHover (int i)
	{
		
		if(!isPressed[i] && !moduleSolved){
			Observe(i);
		}
	}

	void OffButtonHover(int i)
	{
		if(!isPressed[i] && !moduleSolved) {
			buttons[i].GetComponentInChildren<TextMesh>().text = "";
		}
		strike = false;
	}

	void ButtonPress(int i)
	{
		if(strike)
		{
			Observe(i);
			strike = false;
		}

		if(!isPressed[i]){
			numPressed++;
		}
		isPressed[i] = true;
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		buttons[i].AddInteractionPunch();

		buttonText[i] = buttons[i].GetComponentInChildren<TextMesh>().text;
		
		if(numPressed >= 6)
		{
			Check();
		}

		
	}
	void Start () {

		GenerateSolution();
		
	}
	
	

}