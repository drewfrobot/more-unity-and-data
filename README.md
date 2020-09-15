# more-unity-and-data

## Visualising more data in Unity 3D

A further exploration into visualising data in Unity 3D using SQLite. This post won't be the most efficient way to undertake the task; the steps are intended to show the data visualisation journey and not the destination.

The ability to actually explore expansive data landscapes offers new opportunities for intuitive insights over what is possible with traditional tools such as Excel or R.

In an [earlier post](https://github.com/drewfrobot/unity-and-data) the basics of using data stored in a SQLite database in Unity 3D were explored.

Let's recap.

## SQLite and Data in General

SQL or Structured Query Language, often pronounced "sequel", has been around a long time. SQL allows users to make "queries" about data held in relational databases. In simple terms these databases 
```
- store data in tables, 
- each table having one or more columns in common with, or relating to, other tables. 
- an item of data is stored in one location in one table and is linked to, or related to.
- in this way data is not repeated, as is the case in those monolithic single sheet spreadsheets!
```
The power of SQL is in leaving the data alone and using queries to create subsets of the data for visualisation. The storage and viewing the data are separate processes. This provides many benefits
```
- many different visualisations can be created without changing the stored data
- the stored data can be updated and the visualisations automatically update too.
```
A great way to learn SQL is available [here](http://swcarpentry.github.io/sql-novice-survey/). 

Complicated SQL queries will not be needed here, our interest is in utilising SQL to interface with Unity 3D.

SQLite is one SQL tool. It is 
```
- command line driven, with no GUI like other SQL tools such as Microsoft Access.
- disk based, fast, and can handle big datasets.
- free in the public domain and runs locally without servers.
```
This post will use R to create and interface with the SQLite database so the SQLite application itself won't be used.  

## The Data used in this post

Two datasets are used here. Data visualisation is all about being curious and telling stories with data. Linking data different data sets together in visualisations can help reveal hidden stories.

The first data set is from SEDAC, the Socioeconomic and Data Applications Centre - a data centre in NASA's distributed structure. The data gives population density estimates globally and is available [here](https://sedac.ciesin.columbia.edu/data/set/gpw-v4-population-density-rev11). The data is also in the form of a GEOTIFF, where earth is similarly divided into square grids and the colour or raster information of the image gives elevation information for each grid. For this post the 1 degree (approx 110km square) set is chosen. There are separate data sets for the years 2000, 2005, 2010, 2015 and 2020. A login is required to access this data, though it has been included in this repository [here](https://github.com/drewfrobot/unity-and-more-data/tree/master/Assets/Input%20to%20QGIS).

The second data set is simply elevation data of the globe and is from [eAtlas](https://eatlas.org.au/data/uuid/80301676-97fb-4bdf-b06c-e961e5c0cb0b). The data is also in the form of a GEOTIFF. This file is at 30 arc second sampling resolution, and will need downsampling to match the 1 degree resolution of the population density data. This data does not require a login, the GEOTIFF is 1.6GB in size, the direct link is [here](https://eatlas.org.au/pydio/data/public/world_e-atlas-ucsd_srtm30-plus_v8-tif.php)

## The Plan

The hope of this visualisation is to explore how the topography of the earth relates to population density, and to interact with it to explore how it varies. The post is going for a low res, lo-fi Minecraft/Lego feel, because the processing power and time is not great and, well, it should look kinda cool.

The grid size will be 360x180.

## First Steps - QGIS

GIS or Geographic Information System tools are very useful in working with GEOTIFF's and ASCII style GIS files. [QGIS](https://qgis.org/en/site/) is a free and open source tool which is helpful here, where it provides a GUI for GDAL, an open source command line GIS tool. The processing below can also be performed in R, however it is useful to be aware of tools such as QGIS, their visual nature not out of place for this post.

QGIS is also great for obtaining images from Open Street maps with defined extents.

Each GEOTIFF file can be opened via the QGIS browser window as layers and then resampled (if necessary) and converted to a more useful format for our purposes. In this post the output format ASCII XYZ will be chosen, basically an almost CSV with latitude, longitude and then gridded data. 

For ease of this lesson the files have been included in this repository, with suitable attribution.

To resample to lower resolution,
```
- select Raster-> Projections -> Warp (Reproject)
- select the Input Layer
- select the Resampling method (Nearest Neighbour - fast and low-fi)
- leave save to temporary file
- select Run
```
The resampled GEOTIFF will appear as a new layer in the layer window

To convert an original or resampled layer to ASCII XYZ format
```
- select Raster-> Conversion-> Translate (Convert Format)
- select the Input Layer
- select save to file and specify XYZ as file type
- select Run
```
![Pop Density GEOTIFF](https://github.com/drewfrobot/unity-and-more-data/blob/master/Images/gpw-v4-population-density-rev11_1_deg_QGIS.png)
![Elevation GEOTIFF](https://github.com/drewfrobot/unity-and-more-data/blob/master/Images/World_e-Atlas-UCSD_SRTM30-plus_v8_QGIS.jpg)

## Next Steps - R and Data preparation cleaning

R is an interesting tool, like no other. It simply has to be experienced. It has some great packages for processing and visualising data, with some very powerful and very very fast compiled tools.

A great way to learn R is [here](https://software-carpentry.org/lessons/) and [here](https://datacarpentry.org/lessons/)

RStudio is a great development environment for R projects.

First steps are to bring in the data into R. The readr library imports csv files into data frames, and usually needs installing in R before use. Our csv files do not have headers and are separated by whitespace. For ease of this lesson the files have been included in this repository, with suitable attribution.
```
library(readr)
pop_2000 <- read_table2("pop_2000.xyz", col_names = FALSE)
pop_2005 <- read_table2("pop_2005.xyz", col_names = FALSE)
pop_2010 <- read_table2("pop_2010.xyz", col_names = FALSE)
pop_2015 <- read_table2("pop_2015.xyz", col_names = FALSE)
pop_2020 <- read_table2("pop_2020.xyz", col_names = FALSE)
elev_lowres <- read_table2("elev_lowres.xyz", col_names = FALSE)
```
It is very useful to add column names to these data frames.
```
colnames(pop_2000) <- c("lat","long","pop")
colnames(pop_2005) <- c("lat","long","pop")
colnames(pop_2010) <- c("lat","long","pop")
colnames(pop_2015) <- c("lat","long","pop")
colnames(pop_2020) <- c("lat","long","pop")
colnames(elev_lowres) <- c("lat","long","el")
```
Let's add a 'year' column to each population density data frame to identify which year the data is for, and then combine them into one data frame with *rbind* (row bind).
```
pop_2000$year <- 2000
pop_2005$year <- 2005
pop_2010$year <- 2010
pop_2015$year <- 2015
pop_2020$year <- 2020
popdensity_lowres <- rbind(pop_2000,pop_2005,pop_2010,pop_2015,pop_2020)
```
Use the *summary* tool to get a feel for the data
```
summary(popdensity_lowres)
      lat               long             pop                  year     
 Min.   :-179.50   Min.   :-89.50   Min.   :-3.403e+38   Min.   :2000  
 1st Qu.: -89.75   1st Qu.:-44.75   1st Qu.:-3.403e+38   1st Qu.:2005  
 Median :   0.00   Median :  0.00   Median :-3.403e+38   Median :2010  
 Mean   :   0.00   Mean   :  0.00   Mean   :-2.400e+38   Mean   :2010  
 3rd Qu.:  89.75   3rd Qu.: 44.75   3rd Qu.: 0.000e+00   3rd Qu.:2015  
 Max.   : 179.50   Max.   : 89.50   Max.   : 1.364e+04   Max.   :2020  

summary(elev_lowres)
      lat               long              el       
 Min.   :-179.50   Min.   :-89.50   Min.   :-7117  
 1st Qu.: -89.75   1st Qu.:-44.75   1st Qu.:-4250  
 Median :   0.00   Median :  0.00   Median :-2510  
 Mean   :   0.00   Mean   :  0.00   Mean   :-1892  
 3rd Qu.:  89.75   3rd Qu.: 44.75   3rd Qu.:  231  
 Max.   : 179.50   Max.   : 89.50   Max.   : 5344  
```
It can be seen that the *lat* and *long* fields seem quite consistent between the two data frames, and that 
```
- the el or elevation column has below sea level data, where elevation is less than 0.
- the pop or population density column has a large negative number for when no data was available. 
```
Curiously the *dput* tool reveals that the format of the longitude data is quite different between the two data frames. This will not be very useful when the two data frames are *merged* shortly.
```
dput(head(popdensity_lowres))
structure(list(lat = c(-179.5, -178.5, -177.5, -176.5, -175.5, 
-174.5), long = c(89.4999999999999, 89.4999999999999, 89.4999999999999, 
89.4999999999999, 89.4999999999999, 89.4999999999999), pop = c(-3.4028230607371e+38, 
-3.4028230607371e+38, -3.4028230607371e+38, -3.4028230607371e+38, 
-3.4028230607371e+38, -3.4028230607371e+38), year = c(2000, 2000, 
2000, 2000, 2000, 2000)), row.names = c(NA, -6L), class = c("tbl_df", 
"tbl", "data.frame"))

dput(head(elev_lowres))
structure(list(lat = c(-179.5, -178.5, -177.5, -176.5, -175.5, 
-174.5), long = c(89.5, 89.5, 89.5, 89.5, 89.5, 89.5), el = c(-3541, 
-3521, -3499, -3486, -3482, -3473)), row.names = c(NA, -6L), class = c("tbl_df", 
"tbl", "data.frame"))
```
The longitude data format in the *popdensity* data frame can easily be changed to match the format in the *elev* data frame.
```
popdensity_lowres$long<- round(popdensity_lowres$long,digits=1)
```
It's very straight forward in R to plot the data in 3D, not perfectly formatted but gives an indication of how the data looks. The *plotly* library usually needs installing in R.
```
library(plotly)
figp <- plot_ly(pop_2000[pop_2000$pop>0,],x=~lat,y=~long,z=~pop)
figp

figt <- plot_ly(elev_lowres[elev_lowres$el>=0,],x=~lat,y=~long,z=~el)
figt
```
The topographical plot has Antarctica and Greenland at large elevations similar to the Himalayas, the colour information is the same in the GEOTIFF. The population density also has some strange artifacts.

For our purposes, only data which matches the following criteria will be included.
- where there is population density data available.
- where the elevation is above 0.
- where the longitude is greater than -55, which rules out Antartica.

So, let's merge the two dataframes together on latitude and longitude and limit the resulting data which is included
```
opo <- merge(popdensity_lowres,elev_lowres)
poptop <- opo[opo$pop>0&opo$el>=0&opo$long>-55,]
```
The *merge* tool requires only the two data frame names; they have common column names *lat* and *long* which the merging is based on. This *merge* tool is the R equivalent of SQLite's *SELECT* command.

There is now just the one data frame. The next step is to create a SQLite database and add the data frame to it as a SQL table.

Fortunately R has a SQLite interface called RSQLite, and usually needs installing in R before use. [Datacamp](https://www.datacamp.com/community/tutorials/sqlite-in-r) has a great tutorial on using this interface.
```
library(RSQLite)
```
Handy at this point to set the working directory in RStudio as this is where the SQLite database will be created.

Next simply open/create a SQLite database from with R, write the data frame as a table called *poptop_db* and disconnect from the database. Always good to set row names to *Null* too.
```
rownames(poptop) <- c()
conn <- dbConnect(RSQLite::SQLite(), "poptop.db")
dbWriteTable(conn,"poptop_db", poptop,overwrite=TRUE)
dbListTables(conn)
dbDisconnect(conn)
```
This process of connecting to the database, performing an operation and disconnecting again is commonplace. Here a *SELECT* query is run on the table *poptop_db* to return the first 10 rows from the SQLite database.
```
conn <- dbConnect(RSQLite::SQLite(), "poptop.db")
dbGetQuery(conn, "SELECT * FROM poptop_db LIMIT 10")
dbDisconnect(conn)
```
That's all that is needed in R, time for Unity 3D.
## Unity 3D

Once again the SQLite database *poptop.db* summarising all of the above is available in this repository. Let's get the project all setup and then discuss the various components.

### Setup

1.  In a new Unity Project, create a *Plugins* folder in the *Assets* folder.

2.  For a windows machine, download SQLite "dll" and "tools" packages from [here](https://www.sqlite.org/download.html), unzip and place the following files into this newly created *Plugins* folder. 
```
    sqlite3.def
    sqlite3.dll
```
3.  Either add the *poptop.db* created earlier or the downloaded version from the repository to the *Assets* folder.

4.  From the local unity installation at 
    ```
    C:\Program Files\Unity\...\UnityVersion\Editor\Data\Mono\lib\mono\2.0, 
    ```
    copy the following files to the *Asset* folder.
    ``` 
    Mono.Data.Sqlite.dll
    Mono.Data.SqliteClient.dll
    ```
    These files are only available in the installations of Unity up to and including Unity 2018.x.xfx. They are not available in the Unity 2019/2020 installations, but still work. When using Unity 2019/2020, also install an instance of 2018 and copy these files over. Do not use the *Mono bleeding edge* versions, they will not work.
      
5.  Create 3D-> Cube GameObject. Remove Box Collider in the Inspector window, it is not needed.

6.  Drag this to the *Assets* window to create a prefab; rename it *Bar*. 
    Remove the Cube GameObject from the hierarchy view.
    Change the scale of the Bar prefab in the Inspector window to 0.05/0.05/0.05
    Also in the Inspector window give the prefab the Tag *BarPrefabTag*, create as new if necessary.
    
7.  Create an empty GameObject, call it DBAccess.

8.  Create the following UI-> Dropdown elements.
    - YearDropdown. In the Inspector window add options 2000, 2005, 2010, 2015, 2020
    - PlotsDropdown. In the Inspector window add options PopDensity, Elevation, Both
    - MinPopDensityDropdown. In the Inspector window add options >= 0, >= 1, >= 10, >= 100, >= 1000
    - PopBiasDropdown. In the Inspector window add options 1x, 2x, 4x, 8x, 16x

9.  Create a 3D-> Plane GameObject, and change its scale in the Inspector window to 2/2/2.

10. Add a controller to allow navigation of the scene. Download FP All in One from the Asset Store and add just the FirstPersonAIO prefab and script. Drag it into the Hierarchy. Disable the Main Camera in the Inspector Window.

11. Under Preferences->External ensure Visual Studio Community 2017 or 2019 is the External Editor. There is a lot of background processing with continuous compiling and Visual Studio handles this well.

12. Add the DBAccess Script from this repository into the *Assets* folder. Wait for the compiler to finish its work (the spinning circle at the bottom right of the app). Drag the script icon onto the DBAccess game object created earlier in the hierarchy. This attached C# script will be used to manipulate the object during play. 
    
13. In the inspector window for the GameObject DBAccess there is a number of variables. For each variable shown, drag the relevant GameObject/Prefab to the variable box.
    - for the variable barPrefab, drag the "Bar" prefab onto this variable in the Inspector window.
    - for the Dropdown variables, drag the respective Dropdown GameObjects onto the relevant variable in the Inspector Window.
    
14. In each Dropdown GameObject, in the *On Value Changed* element in the Inspector window, drag the DBAccess GameObject onto the None(Object) field, then Select the appropriate function. eg DBAccess-> YearDropdown_IndexChanged for the YearDropdown GameObject. Be careful not to select YearDropdown_IndexChanged (int).

15. Consider adding UI-> Text to describe each Dropdown GameObject.

## The Visualisation

Essentially the visualisation instantiates a prefab cube for each data point, then modifies its length based on the data value. Earth data is coloured green, population density data is coloured red.

There are four parameters which can be adjusted via the headup display dropdowns
- the year for the population density.
- a choice of either just the population density plot, the elevation plot, or both.
- a choice of what is the minimum population density to display.
- a bias multiplier to make the population density more prominent.

Have fun exploring the data!

![Data Vis](https://github.com/drewfrobot/unity-and-more-data/blob/master/Images/vis01.JPG)

## Further reading and inspiration for this exercise

SQLite setup in Unity
https://answers.unity.com/questions/743400/database-sqlite-setup-for-unity.html

Iris data set visualised as spheres from CSV file. Uses transforms which can linger in the scene after play mode stopped.
https://sites.psu.edu/bdssblog/2017/04/06/basic-data-visualization-in-unity-scatterplot-creation/

Mathematical functions eg sine wave, visualised by vary resolution of small cubs which form line graph of function
https://catlikecoding.com/unity/tutorials/basics/building-a-graph/

Scatterplots in 3D in R
http://www.sthda.com/english/wiki/scatterplot3d-3d-graphics-r-software-and-data-visualization
