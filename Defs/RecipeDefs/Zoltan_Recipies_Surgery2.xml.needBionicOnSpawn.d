<?xml version="1.0" encoding="utf-8" ?>
<Defs>

	<RecipeDef ParentName="SurgeryFlesh">
		<defName>InstallOvipositorbis</defName>
		<label>install ovipositor</label>
		<description>Installs ovipositor.</description>
		<!--<workerClass>Recipe_InstallNaturalBodyPart</workerClass>-->
		<workerClass>Recipe_InstallImplant</workerClass>
		<jobString>Installing ovipositor.</jobString>
		<workAmount>2000</workAmount>
		<!--
		<recipeUsers>
			<li>Zoltan</li>
        </recipeUsers>
		-->
		<ingredients>
			<li>
			<filter>
				<categories><li>Medicine</li></categories>
			</filter>
			<count>1</count>
			</li>
			<li>
			<filter>
				<thingDefs><li>LTF_Ovipositorbis</li></thingDefs>
			</filter>
			<count>1</count>
			</li>
		</ingredients>
		
		<fixedIngredientFilter>
			<categories>
				<li>Medicine</li>
			</categories>
			<thingDefs>
				<li>LTF_Ovipositorbis</li>
			</thingDefs>
		</fixedIngredientFilter>
		
		<appliedOnFixedBodyParts>
			<li>Waist</li>
			<!--<li>LTF_Ovipositor</li>-->
		</appliedOnFixedBodyParts>
		
		<addsHediff>Hediff_LTF_Ovipositor</addsHediff>
		
	</RecipeDef>

</Defs>