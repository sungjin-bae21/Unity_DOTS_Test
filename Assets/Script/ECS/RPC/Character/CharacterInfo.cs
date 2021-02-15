using Unity.Entities;
using Unity.Collections;
using Unity.Assertions;
using Unity.NetCode;

public enum CharacterClass
{
    Arthas
}


public struct CharacterInstanceInfo : IRpcCommand
{
    public CharacterClass character_class;
    public FixedString64 skill1;
    public FixedString64 skill2;
    public FixedString64 skill3;
    public FixedString64 skill4;

    public CharacterInstanceInfo(CharacterClass class_,
                                 FixedString64 skill1_,
                                 FixedString64 skill2_,
                                 FixedString64 skill3_,
                                 FixedString64 skill4_)
    {
        character_class = class_;
        skill1 = skill1_;
        skill2 = skill2_;
        skill3 = skill3_;
        skill4 = skill4_;
    }
}
