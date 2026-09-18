# HaDevMon *(Ha-devmon)* - Home Assistant Device Monitor

HaDevMon is a lightweight device monitor designed to work with Home Assistant through MQTT.
It helps automate the lifecycle of devices in a home lab—particularly systems that do not need to run around the clock.

The project was inspired by a media transcoding server in my home lab.
Home Assistant sends a Wake-on-LAN packet when the server is needed, and HaDevMon helps shut the machine down again once its work is complete.

## Architecture

HaDevMon uses a modular architecture.
A central host - the app - coordinates device monitoring and communication components,
while platform and protocol-specific implementations remain isolated behind shared abstractions. 

This keeps the project adaptable as support for additional devices, operating systems, and integrations is added.

## Contributing

Contributions are welcome and appreciated :). 
Whether you would like to report an issue, suggest an improvement, improve documentation, or submit code, your help makes HaDevMon better for everyone.
