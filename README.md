# DirtyCLR
> This research has been possibile with the support of [Shielder](https://www.shielder.com/) who has sponsored this research with the goal to discover new ways of blend-in within legitimate applications and raise awerness about uncovered sophisticated attack venues, contributing to the security of the digital ecosystem.
Shielder invests from 25% to 100% of employees time into Security Research and R&D, whose output can be seen in its [advisories](https://www.shielder.com/advisories/) and [blog](https://www.shielder.com/blog/).
If you like the type of research that is being published, and you would like to uncover unexplored attacks and vulnerabilities, do not hesitate to [reach out](info@shielder.com).

![dirtyclr_logo](https://github.com/ipSlav/DirtyCLR/assets/63005335/12f5dca0-9489-4761-9810-12e92bbaec5d)

An `App Domain Manager Injection` DLL PoC on steroids with a clean Thread Call Stack and no direct WinAPI calls.<br>
More information about this tool can be found in the [Let Me Manage Your AppDomain](https://ipslav.github.io/2023-12-12-let-me-manage-your-appdomain/) blogpost.

## Usage guide:
> This project already contains a `key.snk` and an `enc.bin` msfvenom messagebox payload

1) Create a raw format `.bin` shellcode and encrypt it with `xor3.py`
2) Install microsoft [SDK](https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/) and navigate into `C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools`
3) Run `sn.exe -k key.snk` and copy your new `key.snk` to whatever folder you prefer
4) Open the `DirtyCLR` solution, right click on `Project Properties`, select `Properties`, navigate on `Signing` and then `<Browse...>` to upload your `key.snk`
5) Right click  again on `Project Properties`, select `Add`=>`Existing Item...` to upload your `enc.bin`
6) Click on the newly addedd `enc.bin` and in its `Properties` select `Embedded Resource` from the `Build Action` dropdown menu
7) Save everything and build

## Credits
- Casey Smith (@subTee)
- Charles Hamilton ([@MrUn1k0d3r](https://twitter.com/MrUn1k0d3r))
- Adam Chester ([@\_xpn\_](https://twitter.com/_xpn_))
- [@daem0nc0re](https://twitter.com/daem0nc0re)
- Dylan Tran ([@d_tranman](https://twitter.com/d_tranman))
