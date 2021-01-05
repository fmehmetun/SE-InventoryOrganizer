// Place a Cargo Container & Timer to grid.
// Set interval of timer and action of timer as "Programmable Block -> Run"
// After running once on PB or Timer, program continuously runs itself.
String mainContainerName = "Main Cargo Container";
String mainTimerName = "InventoryOrganizerTimer";

List<String> exclude_block_names = new List<string>{
	"Ejector",
	"Main Cargo Container"
};

// Main classes.
IMyInventory mainInventory;
IMyTimerBlock mainTimer;
Dictionary<IMyTerminalBlock, IMyInventory> dict_tmb_invb;

// Utility methods.
public List<String> split(String x, String m, int count){
	return new List<String>(x.Split(new string[] {m}, count, StringSplitOptions.None));
}
public List<String> split_nc(String x, String m){
	return new List<String>(x.Split(new string[] {m}, StringSplitOptions.None));
}
public void transferEverything(IMyInventory src, IMyInventory dst){
	for(int i=0; i<32; i++){
		dst.TransferItemFrom(src, i, null, true, null);
	}
}
public String getAbsoluteItemName(MyInventoryItem item){
	List<String> dont_append_type = new List<string>{
		"MetalGrid",
		"InteriorPlate",
		"SteelPlate",
		"Girder",
		"SmallTube",
		"LargeTube",
		"Motor",
		"Display",
		"BulletproofGlass",
		"Superconductor",
		"Computer",
		"GravityGenerator",
		"Explosives",
		"Scrap",
		"SolarCell",
		"PowerCell",
		"Canvas"
	};

	var inv_item_str = split(item.ToString(), "/", 2);
	var inv_item_name_str = inv_item_str[1];
	// Component, Ingot, Ore
	var inv_item_type_str = split(inv_item_str[0], "_", 2)[1];

	if(!dont_append_type.Contains(inv_item_name_str)){
		inv_item_name_str += inv_item_type_str;
	}

	return inv_item_name_str;
}

public Program(){
	mainInventory = GridTerminalSystem.GetBlockWithName(mainContainerName).GetInventory();
	mainTimer = (IMyTimerBlock) GridTerminalSystem.GetBlockWithName(mainTimerName);

	List<IMyTerminalBlock> allTerminalBlocks = new List<IMyTerminalBlock>();
	GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(allTerminalBlocks);
	String mainGridName = GridTerminalSystem.GetBlockWithName(mainContainerName).CubeGrid.CustomName;

	dict_tmb_invb = new Dictionary<IMyTerminalBlock, IMyInventory>();

	IMyInventory invb;
	foreach(IMyTerminalBlock tmb in allTerminalBlocks){
		bool exclude = false;
		foreach(String s in exclude_block_names){
			if(tmb.CustomName.Contains(s)){
				exclude = true;
				break;
			}
		}
		if(
			tmb.HasInventory &&
			tmb.CubeGrid.CustomName.Equals(mainGridName) &&
			tmb.InventoryCount > 0 &&
			tmb.CustomName != mainContainerName &&
			!exclude
		){
			if(tmb.InventoryCount == 2){
				invb = tmb.GetInventory(1);
			}else{
				invb = tmb.GetInventory(0);
			}
			if(invb != null && mainInventory.IsConnectedTo(invb)){
				dict_tmb_invb.Add(tmb, invb);
			}
		}
	}
}

public void Main(string argument, UpdateType updateSource)
{
	IMyTerminalBlock tmb;
	IMyInventory invb;

	// Transfer everything to main container.
	foreach(KeyValuePair<IMyTerminalBlock, IMyInventory> b in dict_tmb_invb){
		transferEverything(b.Value, mainInventory);
	}

	List<MyInventoryItem> main_inv_items = new List<MyInventoryItem>();

	foreach(KeyValuePair<IMyTerminalBlock, IMyInventory> b in dict_tmb_invb){
		tmb = b.Key;
		invb = b.Value;
		List<MyInventoryItem> invb_items = new List<MyInventoryItem>();

		mainInventory.GetItems(main_inv_items);

		var lines = split_nc(tmb.CustomData, "\n");
		foreach(String line in lines){
			if(line.Equals("")){continue;}
			var line_split = split_nc(line, ":");
			if(line_split.Count != 2){continue;}

			var w_item_name = line_split[0];
			var w_amount = (float) Int32.Parse(line_split[1]);
			var w_exists = false;
			var invb_item_amount = 0.0f;
			//Echo(tmb.CustomName + " " + w_item_name + " " + w_amount.ToString());

			invb.GetItems(invb_items);
			foreach(MyInventoryItem invb_item in invb_items){
				var invb_item_name = getAbsoluteItemName(invb_item);

				if(w_item_name.Equals(invb_item_name)){
					invb_item_amount = (float) invb_item.Amount;
					if(w_amount > invb_item_amount){
						Echo("Exist not enough: " + invb_item_name + " " + w_amount.ToString());
						break;
					}
					Echo("Exist: " + invb_item_name + " " + w_amount.ToString());
					w_exists = true;

					// If item exists much more than specified, pull it.
					if(invb_item_amount > w_amount){
						Echo("Exist much: " + invb_item_name + " " + invb_item_amount.ToString());
						mainInventory.TransferItemFrom(invb, invb_item, (VRage.MyFixedPoint)(invb_item_amount - w_amount));
					}

					break;
				}
			}

			if(w_exists){continue;}

			// Is wanted item exist in main inventory?
			// If yes, transfer it to current inventory from main.
			foreach(MyInventoryItem invm_item in main_inv_items){
				var invm_item_name = getAbsoluteItemName(invm_item);

				//Echo(invm_item_name + " " + invm_item.Amount.ToString() + " " + w_amount.ToString());

				if(w_item_name.Equals(invm_item_name)){
					if(w_amount <= (float) invm_item.Amount){
						Echo("Found: " + invm_item_name + " " + w_amount.ToString());
						mainInventory.TransferItemTo(invb, invm_item, (VRage.MyFixedPoint)(w_amount - invb_item_amount));
						break;
					}else{
						Echo("Found not enough: " + invm_item_name + " " + w_amount.ToString());
						break;
					}
				}
			}
		}
		invb_items.Clear();
		main_inv_items.Clear();
	}
	mainTimer.StartCountdown();
}
