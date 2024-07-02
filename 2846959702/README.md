# SRTS Trains

![Preview](https://i.imgur.com/F4VG8h4.png)

## Overview

Adds "functional" trains to Rimworld using SRTS Expanded by NECERO and SmashPhil and Decorative Railway Props by PentalimbedP.

## Features

- Trains operate just like SRTS Expanded caravans
- Trains can be used to transport things across both the colony map and world map
- Travel speeds and fuel types based on train type
- New research required
- Custom train sounds
- More trains will be added in the future

## Trains

Below are the trains currently available.

**Travel Speed** indicates how fast the train travels on a world tile. For reference, a vanilla drop pod is 25 speed and the SRTS Expanded Superpod is 1 speed.

---

### Handcar

![Handcar Image](https://i.imgur.com/jbiRL8f.png)

A handcar is a railroad car powered by its passengers, or by people pushing the car from behind. It is mostly used as a railway maintenance of way or mining car, but it was also used for passenger service in some cases.

**Stats**

**Mass Capacity**: 250

**Max Passengers**: 4

**Travel Speed**: 2

**Fuel Type**: Steel

**Fuel Cost per World Tile**: 10

**Notes**: Handcar is currently not rotatable, but this will be updated once PentalimbedP creates a vertical texture. I chose the handcar to use Steel as fuel to simulate laying down new rails or repairing existing rails. The amount of 10 represents a simulated balance between the Steel it would cost to build "new" railways compared to using the railcar on "existing" rails. You could also think of it as the Steel is costs to repair rails damaged by raiders. Willing to change this based on feedback or perhaps create a mod option.

### Frisco 1503

![Frisco 1503 Image](https://i.imgur.com/pB7s09D.png)

Frisco No. 1503 was built by Baldwin Locomotive Works in 1923. In March of 1938, it received stainless steel running boards skirts, retractable coupler pilot and covered pilot deck. In 1948 they removed all the added beauty and it was put into freights service. Scrapped in 1950s.

**Stats**

**Mass Capacity**: 50000

**Max Passengers**: 100

**Travel Speed**: 3

**Fuel Type**: Wood or Chemfuel

**Fuel Cost per World Tile**: 30

### Old Steam Locomotive

![Steam Locomotive Image](https://i.imgur.com/XDlZBGS.png)

A locomotive that burns fuel to heat water, generating steam that moves pistons connected to the wheels.

**Stats**

**Mass Capacity**: 50000

**Max Passengers**: 100

**Travel Speed**: 3

**Fuel Type**: Wood or Chemfuel

**Fuel Cost per World Tile**: 30

**Notes**: This train's texture will be updated to a more Rimworld vanilla art style in the future.

---

## Limitations

With this current implementation of trains relying on SRTS Expanded, there are some limitations with realism.

- Trains aren't required to be placed on rails, **but** you can just self-enforce this requirement using the rails from PentalimbedP's [Decorative Railway Props](https://steamcommunity.com/sharedfiles/filedetails/?id=2844194752).

- Trains can travel on world tiles without rails or railroads, **but** you can self-enforce by using [Roads of the Rim (Continued)](https://steamcommunity.com/sharedfiles/filedetails/?id=2280318231)

- Trains "fly" out of the map.

- Trains use a blank white texture when in launching and landing.

## FAQ

★ **Railways tab is empty**

Research "Metal Railways" and "Steam Locomotives" in the SRTS Trains research tab.

★ **My train doesn't have any functionality**

Make sure you're not using the trains labeled as "decorative". I've included patches for Decorative Railway Props to change the label and description.

★ **Will you add a requirement for trains to use rails?**

Probably not with how trains are currently implemented. I recommend just using the rails from PentalimbedP's [Decorative Railway Props](https://steamcommunity.com/sharedfiles/filedetails/?id=2844194752) and world roads from [Roads of the Rim (Continued)](https://steamcommunity.com/sharedfiles/filedetails/?id=2280318231).

★ **Can you change the train leaving/incoming animation so the train appears to leave the map on the rails?**

Maybe? This would probably require looking deeply into Rimworld's animation logic with "Skyfaller" objects (ie drop pods, shuttles, etc).

---

## Credits

- PentalimbedP for Decorative Railway Props
- Smash Phil for creating SRTS Expanded code
- Neceros for SRTS Expanded
- TechnoFox for creating SRTS, originally based on work by AKreedz 风影!
