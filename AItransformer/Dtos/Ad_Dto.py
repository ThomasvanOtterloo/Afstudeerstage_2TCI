from dataclasses import dataclass


@dataclass
class AdDto:
    messageId: str
    groupId: str
    groupName: str
    traderNumber: str
    traderName: str
    text: str
    image: str
    video: str
