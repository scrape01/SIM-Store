# SIM-Store
Save small files to a SIM (subscriber identity module or subscriber identification module) card's ADN (Abbreviated Dialling Numbers) file.

Most modern SIM cards can store 128 to 256 kilobytes of user-writable data. Of that, approximately 7 kilobytes is available for ADN. ADN storge is typically not used in modern smartphones, which store contacts using the phone's internal memory.

[logo]: https://github.com/scrape01/SIM-Store/blob/master/images/form.png "SIM Store form"

## Project Prerequisites
* Windows 10
* Visual Studio 2019
* WinSCard.dll
* Log4Net
* USB smartcard reader
* SIM card

## Installing & Running
1. Clone or download this repository to your local machine
2. Open the SIMStore.sln with Visual Studio 2019
3. Compile in Visual Studio 2019
4. Connect your USB smartcard reader & insert SIM
5. Run the project
6. Upload a small (less than 7KB file) to the SIM card

## Current Limitations
1. Files are not transportable from little to big endian computers

## Acknowledgments
* [cardpeek](https://github.com/L1L1/cardpeek) - tool to read the contents of ISO7816 smart cards
* [comex-project](https://github.com/armando-basile/comex-project) - Comex Project is a cross platform suite to exchange commands with smartcard
* [pcsc-sharp](https://github.com/danm-de/pcsc-sharp) -  Personal Computer/Smart Card Resource Manager
