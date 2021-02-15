using Unity.NetCode;
using Unity.Collections;

public struct ExcuteSkillRpc : IRpcCommand
{
    public FixedString64 skill_name;

    public ExcuteSkillRpc(FixedString64 skill_name_)
    {
        skill_name = skill_name_;
    }
}
