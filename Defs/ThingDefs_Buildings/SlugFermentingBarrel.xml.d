<?xml version="1.0" encoding="utf-8" ?>
<Defs>

<ThingDef ParentName="BuildingBase">
    <defName>LTF_SlugFermentingBarrel</defName>
    <label>slug fermenting barrel</label>
	<description>A barrel for fermenting slug dew into liquor.</description>
    <thingClass>Building_FermentingBarrel</thingClass>
    <graphicData>
      <texPath>Things/Building/Production/FermentingBarrel</texPath>
      <graphicClass>Graphic_Multi</graphicClass>
	  <color>(30,120,50)</color>
      <damageData>
        <rect>(0.05,0.1,0.9,0.9)</rect>
      </damageData>
    </graphicData>
    <minifiedDef>MinifiedFurniture</minifiedDef>
    <altitudeLayer>Building</altitudeLayer>
    <passability>PassThroughOnly</passability>
    <fillPercent>0.45</fillPercent>
    <pathCost>60</pathCost>
	<statBases>
		<WorkToBuild>600</WorkToBuild>
		<Mass>10</Mass>
		<MaxHitPoints>100</MaxHitPoints>
		<Flammability>1.0</Flammability>
	</statBases>
    
    <costList>
		<Steel>5</Steel>
		<WoodLog>15</WoodLog>
		<LTF_SlugDew>15</LTF_SlugDew>
    </costList>
	<comps>
		<li Class="CompProperties_TemperatureRuinable">
			<minSafeTemperature>-1</minSafeTemperature>
			<maxSafeTemperature>32</maxSafeTemperature>
			<progressPerDegreePerTick>0.001</progressPerDegreePerTick>
		</li>
	</comps>
    <tickerType>Rare</tickerType>
    <rotatable>true</rotatable>
    <designationCategory>Production</designationCategory>
    <constructEffect>ConstructWood</constructEffect>
	
    <researchPrerequisites><li>LTF_Research_Slug</li></researchPrerequisites>
  </ThingDef>
  
  </Defs>