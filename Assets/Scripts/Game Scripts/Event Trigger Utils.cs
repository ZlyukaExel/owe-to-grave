using UnityEngine.EventSystems;
using UnityEngine.Events;

public static class EventTriggerUtils
{
	public static void AddPointerUpAndDownListeners(this EventTrigger eventTrigger, UnityAction downAction, UnityAction upAction)
	{
		if (eventTrigger == null) return;

		AddListener(eventTrigger, EventTriggerType.PointerDown, downAction);
		AddListener(eventTrigger, EventTriggerType.PointerUp, upAction);
	}

	public static void AddListener(this EventTrigger eventTrigger, EventTriggerType triggerType, UnityAction action)
	{
		var entry = new EventTrigger.Entry
		{
			eventID = triggerType
		};
		entry.callback.AddListener((data) => action());
		eventTrigger.triggers.Add(entry);
	}

	public static void ClearListeners(this EventTrigger eventTrigger, EventTriggerType triggerType)
	{
		eventTrigger.triggers.RemoveAll(entry => entry.eventID == triggerType);
	}
}