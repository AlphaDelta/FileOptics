using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileOptics.TESV
{
    public enum ChangeFormFlagCLAS
    {
        CHANGE_CLASS_TAG_SKILLS = 0x00000002,
    }
    public enum ChangeFormFlagFACT
    {
        CHANGE_FACTION_FLAGS = 0x00000002,
        CHANGE_FACTION_REACTIONS = 0x00000004,
        CHANGE_FACTION_CRIME_COUNTS = unchecked((int)0x80000000),
    }
    public enum ChangeFormFlagACTI
    {
        CHANGE_TALKING_ACTIVATOR_SPEAKER = 0x00800000,
    }
    public enum ChangeFormFlagBOOK
    {
        CHANGE_BOOK_TEACHES = 0x00000020,
        CHANGE_BOOK_READ = 0x00000040,
    }
    public enum ChangeFormFlagDOOR
    {
        CHANGE_DOOR_EXTRA_TELEPORT = 0x00020000,
    }
    public enum ChangeFormFlagINGR
    {
        CHANGE_INGREDIENT_USE = unchecked((int)0x80000000),
    }
    public enum ChangeFormFlagNPC_
    {
        CHANGE_ACTOR_BASE_DATA = 0x00000002,
        CHANGE_ACTOR_BASE_ATTRIBUTES = 0x00000004,
        CHANGE_ACTOR_BASE_AIDATA = 0x00000008,
        CHANGE_ACTOR_BASE_SPELLLIST = 0x00000010,
        CHANGE_ACTOR_BASE_FULLNAME = 0x00000020,
        CHANGE_ACTOR_BASE_FACTIONS = 0x00000040,
        CHANGE_NPC_SKILLS = 0x00000200,
        CHANGE_NPC_CLASS = 0x00000400,
        CHANGE_NPC_FACE = 0x00000800,
        CHANGE_NPC_DEFAULT_OUTFIT = 0x00001000,
        CHANGE_NPC_SLEEP_OUTFIT = 0x00002000,
        CHANGE_NPC_GENDER = 0x01000000,
        CHANGE_NPC_RACE = 0x02000000,
    }
    public enum ChangeFormFlagLVLN
    {
        CHANGE_LEVELED_LIST_ADDED_OBJECT = unchecked((int)0x80000000),
    }
    public enum ChangeFormFlagNOTE
    {
        CHANGE_NOTE_READ = unchecked((int)0x80000000),
    }
    public enum ChangeFormFlagCELL
    {
        CHANGE_CELL_FLAGS = 0x00000002,
        CHANGE_CELL_FULLNAME = 0x00000004,
        CHANGE_CELL_OWNERSHIP = 0x00000008,
        CHANGE_CELL_EXTERIOR_SHORT = 0x10000000,
        CHANGE_CELL_EXTERIOR_CHAR = 0x20000000,
        CHANGE_CELL_DETACHTIME = 0x40000000,
        CHANGE_CELL_SEENDATA = unchecked((int)0x80000000),
    }
    public enum ChangeFormFlagREFR
    {
        CHANGE_FORM_FLAGS = 0x00000001,
        CHANGE_REFR_MOVE = 0x00000002,
        CHANGE_REFR_HAVOK_MOVE = 0x00000004,
        CHANGE_REFR_CELL_CHANGED = 0x00000008,
        CHANGE_REFR_SCALE = 0x00000010,
        CHANGE_REFR_INVENTORY = 0x00000020,
        CHANGE_REFR_EXTRA_OWNERSHIP = 0x00000040,
        CHANGE_REFR_BASEOBJECT = 0x00000080,
        CHANGE_REFR_PROMOTED = 0x02000000,
        CHANGE_REFR_EXTRA_ACTIVATING_CHILDREN = 0x04000000,
        CHANGE_REFR_LEVELED_INVENTORY = 0x08000000,
        CHANGE_REFR_ANIMATION = 0x10000000,
        CHANGE_REFR_EXTRA_ENCOUNTER_ZONE = 0x20000000,
        CHANGE_REFR_EXTRA_CREATED_ONLY = 0x40000000,
        CHANGE_REFR_EXTRA_GAME_ONLY = unchecked((int)0x80000000),
        CHANGE_OBJECT_EXTRA_ITEM_DATA = 0x00000400,
        CHANGE_OBJECT_EXTRA_AMMO = 0x00000800,
        CHANGE_OBJECT_EXTRA_LOCK = 0x00001000,
        CHANGE_OBJECT_EMPTY = 0x00200000,
        CHANGE_OBJECT_OPEN_DEFAULT_STATE = 0x00400000,
        CHANGE_OBJECT_OPEN_STATE = 0x00800000,
    }
    public enum ChangeFormFlagACHR
    {
        CHANGE_FORM_FLAGS = 0x00000001,
        CHANGE_REFR_MOVE = 0x00000002,
        CHANGE_REFR_HAVOK_MOVE = 0x00000004,
        CHANGE_REFR_CELL_CHANGED = 0x00000008,
        CHANGE_REFR_SCALE = 0x00000010,
        CHANGE_REFR_INVENTORY = 0x00000020,
        CHANGE_REFR_EXTRA_OWNERSHIP = 0x00000040,
        CHANGE_REFR_BASEOBJECT = 0x00000080,
        CHANGE_REFR_PROMOTED = 0x02000000,
        CHANGE_REFR_EXTRA_ACTIVATING_CHILDREN = 0x04000000,
        CHANGE_REFR_LEVELED_INVENTORY = 0x08000000,
        CHANGE_REFR_ANIMATION = 0x10000000,
        CHANGE_REFR_EXTRA_ENCOUNTER_ZONE = 0x20000000,
        CHANGE_REFR_EXTRA_CREATED_ONLY = 0x40000000,
        CHANGE_REFR_EXTRA_GAME_ONLY = unchecked((int)0x80000000),
        CHANGE_ACTOR_LIFESTATE = 0x00000400,
        CHANGE_ACTOR_EXTRA_PACKAGE_DATA = 0x00000800,
        CHANGE_ACTOR_EXTRA_MERCHANT_CONTAINER = 0x00001000,
        CHANGE_ACTOR_EXTRA_DISMEMBERED_LIMBS = 0x00020000,
        CHANGE_ACTOR_LEVELED_ACTOR = 0x00040000,
        CHANGE_ACTOR_DISPOSITION_MODIFIERS = 0x00080000,
        CHANGE_ACTOR_TEMP_MODIFIERS = 0x00100000,
        CHANGE_ACTOR_DAMAGE_MODIFIERS = 0x00200000,
        CHANGE_ACTOR_OVERRIDE_MODIFIERS = 0x00400000,
        CHANGE_ACTOR_PERMANENT_MODIFIERS = 0x00800000,
    }
    public enum ChangeFormFlagINFO
    {
        CHANGE_TOPIC_SAIDONCE = unchecked((int)0x80000000),
    }
    public enum ChangeFormFlagQUST
    {
        CHANGE_QUEST_FLAGS = 0x00000002,
        CHANGE_QUEST_SCRIPT_DELAY = 0x00000004,
        CHANGE_QUEST_ALREADY_RUN = 0x04000000,
        CHANGE_QUEST_INSTANCES = 0x08000000,
        CHANGE_QUEST_RUNDATA = 0x10000000,
        CHANGE_QUEST_OBJECTIVES = 0x20000000,
        CHANGE_QUEST_SCRIPT = 0x40000000,
        CHANGE_QUEST_STAGES = unchecked((int)0x80000000),
    }
    public enum ChangeFormFlagPACK
    {
        CHANGE_PACKAGE_WAITING = 0x40000000,
        CHANGE_PACKAGE_NEVER_RUN = unchecked((int)0x80000000),
    }
    public enum ChangeFormFlagFLST
    {
        CHANGE_FORM_LIST_ADDED_FORM = unchecked((int)0x80000000),
    }
    public enum ChangeFormFlagECZN
    {
        CHANGE_ENCOUNTER_ZONE_FLAGS = 0x00000002,
        CHANGE_ENCOUNTER_ZONE_GAME_DATA = unchecked((int)0x80000000),
    }
    public enum ChangeFormFlagLCTN
    {
        CHANGE_LOCATION_KEYWORDDATA = 0x40000000,
        CHANGE_LOCATION_CLEARED = unchecked((int)0x80000000),
    }
    public enum ChangeFormFlagSMQN
    {
        CHANGE_QUEST_NODE_TIME_RUN = unchecked((int)0x80000000),
    }
    public enum ChangeFormFlagRELA
    {
        CHANGE_RELATIONSHIP_DATA = 0x00000002,
    }
    public enum ChangeFormFlagSCEN
    {
        CHANGE_SCENE_ACTIVE = unchecked((int)0x80000000),
    }
    public enum ChangeFormFlagDefault
    {
        CHANGE_FORM_FLAGS = 0x00000001,
        CHANGE_BASE_OBJECT_VALUE = 0x00000002,
        CHANGE_BASE_OBJECT_FULLNAME = 0x00000004
    }

    public static class ChangeFormFlagEnums
    {
        public static Type GetEnum(ChangeFormType type)
        {
            switch (type)
            {
                case ChangeFormType.CLAS:
                    return typeof(ChangeFormFlagCLAS);
                case ChangeFormType.FACT:
                    return typeof(ChangeFormFlagFACT);
                case ChangeFormType.ACTI:
                    return typeof(ChangeFormFlagACTI);
                case ChangeFormType.BOOK:
                    return typeof(ChangeFormFlagBOOK);
                case ChangeFormType.DOOR:
                    return typeof(ChangeFormFlagDOOR);
                case ChangeFormType.INGR:
                    return typeof(ChangeFormFlagINGR);
                case ChangeFormType.NPC_:
                    return typeof(ChangeFormFlagNPC_);
                case ChangeFormType.LVLN:
                case ChangeFormType.LVLI:
                case ChangeFormType.LVSP:
                    return typeof(ChangeFormFlagLVLN);
                case ChangeFormType.NOTE:
                    return typeof(ChangeFormFlagNOTE);
                case ChangeFormType.CELL:
                    return typeof(ChangeFormFlagCELL);
                case ChangeFormType.REFR:
                case ChangeFormType.PMIS:
                case ChangeFormType.PARW:
                case ChangeFormType.PGRE:
                case ChangeFormType.PBEA:
                case ChangeFormType.PFLA:
                case ChangeFormType.PCON:
                case ChangeFormType.PBAR:
                case ChangeFormType.PHZD:
                    return typeof(ChangeFormFlagREFR);
                case ChangeFormType.ACHR:
                    return typeof(ChangeFormFlagACHR);
                case ChangeFormType.INFO:
                    return typeof(ChangeFormFlagINFO);
                case ChangeFormType.QUST:
                    return typeof(ChangeFormFlagQUST);
                case ChangeFormType.PACK:
                    return typeof(ChangeFormFlagPACK);
                case ChangeFormType.FLST:
                    return typeof(ChangeFormFlagFLST);
                case ChangeFormType.ECZN:
                    return typeof(ChangeFormFlagECZN);
                case ChangeFormType.LCTN:
                    return typeof(ChangeFormFlagLCTN);
                case ChangeFormType.SMQN:
                    return typeof(ChangeFormFlagSMQN);
                case ChangeFormType.RELA:
                    return typeof(ChangeFormFlagRELA);
                case ChangeFormType.SCEN:
                    return typeof(ChangeFormFlagSCEN);
                default:
                    return typeof(ChangeFormFlagDefault);
            }
        }
    }
}
