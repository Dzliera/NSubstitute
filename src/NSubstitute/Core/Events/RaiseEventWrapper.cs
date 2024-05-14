using System.Reflection;
using NSubstitute.Exceptions;

namespace NSubstitute.Core.Events;

public abstract class RaiseEventWrapper
{
    protected abstract object?[] WorkOutRequiredArguments(ICall call);
    protected abstract string RaiseMethodName { get; }

    protected EventArgs GetDefaultForEventArgType(Type type)
    {
        if (type == typeof(EventArgs)) return EventArgs.Empty;

        var defaultConstructor = GetPublicDefaultConstructor(type) ?? GetNonPublicDefaultConstructor(type);
        if (defaultConstructor is null)
        {
            var message = string.Format(
                "Cannot create {0} for this event as it has no default constructor. " +
                "Provide arguments for this event by calling {1}({0})."
                , type.Name, RaiseMethodName);
            throw new CannotCreateEventArgsException(message);
        }
        return (EventArgs)defaultConstructor.Invoke([]);
    }

    private static ConstructorInfo? GetPublicDefaultConstructor(Type type)
        => GetDefaultConstructor(type, BindingFlags.Public);
    private static ConstructorInfo? GetNonPublicDefaultConstructor(Type type)
        => GetDefaultConstructor(type, BindingFlags.NonPublic);
    private static ConstructorInfo? GetDefaultConstructor(Type type, BindingFlags bindingFlags)
        => type.GetConstructor(BindingFlags.Instance | bindingFlags, null, Type.EmptyTypes, null);

    protected static void RaiseEvent(RaiseEventWrapper wrapper)
    {
        var context = SubstitutionContext.Current;
        context.ThreadContext.SetPendingRaisingEventArgumentsFactory(call => wrapper.WorkOutRequiredArguments(call));
    }
}