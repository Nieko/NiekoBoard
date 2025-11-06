# NiekoBoard - Powershell powered status dashboard

NiekoBoard is a web and desktop application to display simple dashboard with status' provided via user powershell scripts. A windows service
picks up all powershell files sitting in a folder and presents their status and actions to a client via a web server and then into a client 
dashboard.

__NOTE - PLEASE READ_\_
This dashboard can, with some minor poorly thought-through manual recofigurations, allow for any user to drop a powershell file into a folder and have it
executed periodically as administrator. If this doesn't raise the hairs on the back of your Cyber Security neck, this project may not be
for you.

# Standard setup

The assumed setup is 4-tier system:
* Client machine with user application via browser or desktop app
* gPRC Server
* Windows Service, same machine as gPRC, comms via named pipes
* SQL Database (settings / history) and File Folder containing powershell scripts.

┌─────────────────────────┐  ┌───────────────────────────────┐
│                         │  │                               │
│      Client Desktop     │  │      Server                   │
│ ┌────────────────────┐  │  │                               │
│ │  Desktop Client    │  │  │  ┌─────────────────────────┐  │
│ │                    │  │  │  │  IIS         ┌────────┐ │  │
│ │                    │  │  │  │              │ gPRC   │ │  │
│ │                 ◄──┼──┼──│──┼──────────────┼►Server │ │  │
│ └────────────────────┘  │  │  │              │        │ │  │
│ ┌──────────────────────────│──│─────────────┐│        │ │  │
│ │  Browser              │  │  │ Static Html ││        │ │  │
│ │                       │  │  │ Server      ││        │ │  │
│ │                       │  │  │             ││        │ │  │
│ │                       │  │  │           ◄─┼┼─►   ▲ ▲│ │  │
│ └──────────────────────────│──│─────────────┘│     │ ││ │  │
│                         │  │  │              └─────┼─┼┘ │  │
└─────────────────────────┘  │  └────────────────────┼─┼──┘  │
                             │  ┌────────────────────┼┐│     │
                             │  │  Windows Service   │││     │
                             │  │                    ▼││     │
                             │  │                     ││     │
                             │  │          (Polling) ▲││     │
                             │  └────────────────────┼┘│     │
                             │  ┌────────────┐┌──────┼─┼──┐  │
                             │  │            ││      │ │  │  │
                             │  │  File      ││      ▼ ▼  │  │
                             │  │            ││           │  │
                             │  │            ││  Database │  │
                             │  │            ││           │  │
                             │  │            ││           │  │
                             │  │            ││           │  │
                             │  │            ││           │  │
                             │  │            ││           │  │
                             │  │            ││           │  │
                             │  └────────────┘└───────────┘  │
                             └───────────────────────────────┘
# Version Changes

## 0.2.0.0
Pre-Alpha buildable and testable concept
