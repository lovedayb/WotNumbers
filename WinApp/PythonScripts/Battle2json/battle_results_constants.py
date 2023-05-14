# uncompyle6 version 3.7.4
# Python bytecode 2.7 (62211)
# Decompiled from: Python 2.7.8 (default, Jun 30 2014, 16:08:48) [MSC v.1500 64 bit (AMD64)]
# Embedded file name: scripts/common/battle_results/battle_results_constants.py
# from chat_shared import SYS_MESSAGE_TYPE
# from constants import ARENA_BONUS_TYPE
class ARENA_BONUS_TYPE:
    UNKNOWN = 0
    REGULAR = 1
    TRAINING = 2
    TOURNAMENT = 4
    CLAN = 5
    CYBERSPORT = 7
    EVENT_BATTLES = 9
    GLOBAL_MAP = 13
    TOURNAMENT_REGULAR = 14
    TOURNAMENT_CLAN = 15
    FALLOUT_CLASSIC = 18
    FALLOUT_MULTITEAM = 19
    SORTIE_2 = 20
    FORT_BATTLE_2 = 21
    RANKED = 22
    BOOTCAMP = 23
    EPIC_RANDOM = 24
    EPIC_RANDOM_TRAINING = 25
    EVENT_BATTLES_2 = 26
    EPIC_BATTLE = 27
    EPIC_BATTLE_TRAINING = 28
    BATTLE_ROYALE_SOLO = 29
    BATTLE_ROYALE_SQUAD = 30
    TOURNAMENT_EVENT = 31
    BOB = 32
    EVENT_RANDOM = 33
    BATTLE_ROYALE_TRN_SOLO = 34
    BATTLE_ROYALE_TRN_SQUAD = 35
    WEEKEND_BRAWL = 36
    MAPBOX = 37
    MAPS_TRAINING = 38
    RTS = 39
    RTS_1x1 = 40
    RTS_BOOTCAMP = 41
    FUN_RANDOM = 42
    COMP7 = 43
    WINBACK = 44
    RANGE = (
     UNKNOWN, REGULAR, TRAINING, TOURNAMENT, CLAN, CYBERSPORT, EVENT_BATTLES, EVENT_BATTLES_2, GLOBAL_MAP,
     TOURNAMENT_REGULAR, TOURNAMENT_CLAN,
     FALLOUT_CLASSIC, FALLOUT_MULTITEAM, BOOTCAMP, SORTIE_2, FORT_BATTLE_2, RANKED,
     EPIC_RANDOM, EPIC_RANDOM_TRAINING, EPIC_BATTLE, EPIC_BATTLE_TRAINING, TOURNAMENT_EVENT, EVENT_RANDOM,
     BATTLE_ROYALE_SOLO, BATTLE_ROYALE_SQUAD, BOB, BATTLE_ROYALE_TRN_SOLO, BATTLE_ROYALE_TRN_SQUAD,
     MAPBOX, WEEKEND_BRAWL, MAPS_TRAINING, RTS, RTS_1x1, RTS_BOOTCAMP, FUN_RANDOM, COMP7, WINBACK)
    RANDOM_RANGE = (
     REGULAR, EPIC_RANDOM)
    FALLOUT_RANGE = (FALLOUT_CLASSIC, FALLOUT_MULTITEAM)
    TOURNAMENT_RANGE = (TOURNAMENT, TOURNAMENT_REGULAR, TOURNAMENT_CLAN, TOURNAMENT_EVENT)
    BATTLE_ROYALE_RANGE = (BATTLE_ROYALE_SOLO, BATTLE_ROYALE_SQUAD, BATTLE_ROYALE_TRN_SOLO, BATTLE_ROYALE_TRN_SQUAD)
    BATTLE_ROYALE_REGULAR_RANGE = (BATTLE_ROYALE_SOLO, BATTLE_ROYALE_SQUAD)
    BATTLE_ROYALE_SQUAD_RANGE = (BATTLE_ROYALE_SQUAD, BATTLE_ROYALE_TRN_SQUAD)
    RTS_RANGE = (RTS, RTS_1x1, RTS_BOOTCAMP)
    RTS_BATTLES = (RTS, RTS_1x1)
    EXTERNAL_RANGE = (
     SORTIE_2, FORT_BATTLE_2, GLOBAL_MAP,
     TOURNAMENT, TOURNAMENT_CLAN, TOURNAMENT_REGULAR, TOURNAMENT_EVENT)
PATH_TO_CONFIG = {ARENA_BONUS_TYPE.REGULAR: '.random', 
   ARENA_BONUS_TYPE.EPIC_RANDOM: '.random', 
   ARENA_BONUS_TYPE.EPIC_RANDOM_TRAINING: '.random', 
   ARENA_BONUS_TYPE.WINBACK: '.random', 
   ARENA_BONUS_TYPE.RANKED: '.ranked', 
   ARENA_BONUS_TYPE.EPIC_BATTLE: '.frontline', 
   ARENA_BONUS_TYPE.EPIC_BATTLE_TRAINING: '.frontline', 
   ARENA_BONUS_TYPE.BATTLE_ROYALE_SOLO: '.battle_royale', 
   ARENA_BONUS_TYPE.BATTLE_ROYALE_SQUAD: '.battle_royale', 
   ARENA_BONUS_TYPE.BATTLE_ROYALE_TRN_SOLO: '.battle_royale', 
   ARENA_BONUS_TYPE.BATTLE_ROYALE_TRN_SQUAD: '.battle_royale', 
   ARENA_BONUS_TYPE.MAPBOX: '.random', 
   ARENA_BONUS_TYPE.MAPS_TRAINING: '.maps_training', 
   ARENA_BONUS_TYPE.FUN_RANDOM: '.random', 
   ARENA_BONUS_TYPE.COMP7: '.comp7'}
POSSIBLE_TYPES = (
 int, float, str, bool, list, tuple, dict, set, None)

class BATTLE_RESULT_ENTRY_TYPE:
    COMMON = 1
    ACCOUNT_SELF = 2
    ACCOUNT_ALL = 3
    VEHICLE_SELF = 4
    VEHICLE_ALL = 5
    PLAYER_INFO = 6
    SERVER = 7
    ALL = (
     COMMON, ACCOUNT_SELF, ACCOUNT_ALL, VEHICLE_SELF, VEHICLE_ALL, PLAYER_INFO, SERVER)


# ARENA_BONUS_TYPE_TO_SYS_MESSAGE_TYPE = {ARENA_BONUS_TYPE.REGULAR: SYS_MESSAGE_TYPE.battleResults.index()}