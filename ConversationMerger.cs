using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml;
using XRL.World;
using HarmonyLib;

namespace ConversationMerger.Extensions
{
	public static class ConversationExtensions
	{
		public static Dictionary<Conversation, bool> LoadingConversations = new Dictionary<Conversation, bool>();
	}

	public static class ConversationNodeExtensions
	{
		public static Dictionary<ConversationNode, bool> LoadingConversationNodes = new Dictionary<ConversationNode, bool>();
		public static bool canMerge(this ConversationNode ourNode, ConversationNode otherNode)
		{
			if(otherNode.ID != ourNode.ID) return false;
			if(otherNode.IfFinishedQuest != ourNode.IfFinishedQuest) return false;
			if(otherNode.IfFinishedQuestStep != ourNode.IfFinishedQuestStep) return false;
			if(otherNode.IfHasBlueprint != ourNode.IfHasBlueprint) return false;
			if(otherNode.IfHaveItemWithID != ourNode.IfHaveItemWithID) return false;
			if(otherNode.IfHaveObservation != ourNode.IfHaveObservation) return false;
			if(otherNode.IfHaveObservationWithTag != ourNode.IfHaveObservationWithTag) return false;
			if(otherNode.IfHavePart != ourNode.IfHavePart) return false;
			if(otherNode.IfHaveQuest != ourNode.IfHaveQuest) return false;
			if(otherNode.IfHaveState != ourNode.IfHaveState) return false;
			if(otherNode.IfHaveSultanNoteWithTag != ourNode.IfHaveSultanNoteWithTag) return false;
			if(otherNode.IfLevelLessOrEqual != ourNode.IfLevelLessOrEqual) return false;
			if(otherNode.IfNotFinishedQuest != ourNode.IfNotFinishedQuest) return false;
			if(otherNode.IfNotFinishedQuestStep != ourNode.IfNotFinishedQuestStep) return false;
			if(otherNode.IfNotHavePart != ourNode.IfNotHavePart) return false;
			if(otherNode.IfNotHaveQuest != ourNode.IfNotHaveQuest) return false;
			if(otherNode.IfNotHaveState != ourNode.IfNotHaveState) return false;
			if(otherNode.IfWearingBlueprint != ourNode.IfWearingBlueprint) return false;
			if(otherNode.SpecialRequirement != ourNode.SpecialRequirement) return false;
			if(otherNode.IfWearingBlueprint !=  ourNode.IfWearingBlueprint) return false;
			if(otherNode.IfTestState != ourNode.IfTestState) return false;
			if(otherNode.IfWearingBlueprint != ourNode.IfWearingBlueprint) return false;
			return true;
		}
		public static void Merge(this ConversationNode ourNode, ConversationNode otherNode)
		{
			// Do merging
			if (otherNode.Text != null) ourNode.Text = otherNode.Text;
			if (otherNode.AddIntState != null) ourNode.AddIntState = otherNode.AddIntState;
			if (otherNode.ClearOwner != null) ourNode.ClearOwner = otherNode.ClearOwner;
			if (otherNode.CompleteQuestStep != null) ourNode.CompleteQuestStep = otherNode.CompleteQuestStep;
			if (otherNode.Filter != null) ourNode.Filter = otherNode.Filter;
			if (otherNode.FinishQuest != null) ourNode.FinishQuest = otherNode.FinishQuest;
			if (otherNode.GiveItem != null) ourNode.GiveItem = otherNode.GiveItem;
			if (otherNode.GiveOneItem != null) ourNode.GiveOneItem = otherNode.GiveOneItem;
			if (otherNode.RevealMapNoteId != null) ourNode.RevealMapNoteId = otherNode.RevealMapNoteId;
			if (otherNode.SetBooleanState != null) ourNode.SetBooleanState = otherNode.SetBooleanState;
			if (otherNode.SetIntState != null) ourNode.SetIntState = otherNode.SetIntState;
			if (otherNode.SetStringState != null) ourNode.SetStringState = otherNode.SetStringState;
			if (otherNode.StartQuest != null) ourNode.StartQuest = otherNode.StartQuest;
			if (otherNode.TakeItem != null) ourNode.TakeItem = otherNode.TakeItem;
			if (otherNode.ToggleBooleanState != null) ourNode.ToggleBooleanState = otherNode.ToggleBooleanState;
			if (otherNode.TradeNote) ourNode.TradeNote = otherNode.TradeNote;
			if (otherNode.Choices != null)
			{
				if (ourNode.Choices == null) ourNode.Choices = otherNode.Choices;
				else
				{
					int nextOrdinal = ourNode.Choices.Count; // this should hopefully be accurate
					foreach (ConversationChoice choice in otherNode.Choices)
					{
						choice.Ordinal = nextOrdinal;
						ourNode.Choices.Add(choice);
						nextOrdinal++;
					}
				}
			}
		}
		public static void setMerging(ConversationNode Node, XmlTextReader Reader)
		{
			if(Reader.GetAttribute("Load") == "Merge")
			{
				ConversationNodeExtensions.LoadingConversationNodes.Add(Node, true);
			}
		}
	}
}

/// <summary>
/// Patches ConversationLoader.LoadConversationNodeNode to load the Load attribute
/// </summary>
namespace ConversationMerger.HarmonyPatches{
	using ConversationMerger.Extensions;
	[HarmonyPatch(typeof(ConversationLoader), "LoadConversationNodeNode")]
	public class LoadConversationNodeNode_Patcher
	{
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var code = new List<CodeInstruction>(instructions);

			// We'll need to modify code here.
			int insertionIndex = -1;
			for (int i = 0; i < code.Count; i++)
			{
				if (code[i].opcode == OpCodes.Stfld && ((FieldInfo)code[i].operand).Name == "TradeNote")
				{
					insertionIndex = i+1;
					break;
				}
			}
			var instructionsToInsert = new List<CodeInstruction>();
			instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloc_0));
			instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_1));
			instructionsToInsert.Add(new CodeInstruction(OpCodes.Callvirt, (MethodInfo)AccessTools.Method(typeof(ConversationNodeExtensions), "setMerging")));
			if (insertionIndex != -1)
			{
				code.InsertRange(insertionIndex, instructionsToInsert);
			}
			return code;
		}
	}
	
	[HarmonyPatch(typeof(ConversationLoader), "LoadConversationNode")]
	public class LoadConversationNodePatch {
		public static bool Prefix(ref Conversation __result, ConversationLoader __instance, XmlTextReader Reader) {
			Conversation conversation;
			string conversation_ID = Reader.GetAttribute("ID");
			if (Reader.GetAttribute("Load") == "Merge")
			{
				conversation = __instance.ConversationsByID[conversation_ID];
				ConversationExtensions.LoadingConversations[conversation] = true;
			}
			else
			{
				conversation = new Conversation();
				conversation.ID = conversation_ID;
			}
			if (Reader.NodeType == XmlNodeType.EndElement)
			{
				__result = conversation;
				return false;
			}
			while (Reader.Read())
			{
				if (Reader.Name == "node")
				{
					ConversationNode conversationNode = (ConversationNode)AccessTools.Method(typeof(ConversationLoader), "LoadConversationNodeNode").Invoke(__instance, new object[]{Reader, conversation});
					conversationNode.ParentConversation = conversation;
					if (conversationNode.ID == "Start")
					{
						var haveMerged = false;
						if(ConversationNodeExtensions.LoadingConversationNodes.ContainsKey(conversationNode))
						{
							foreach(ConversationNode startNode in conversation.StartNodes)
							{
								if(startNode.canMerge(conversationNode))
								{
									startNode.Merge(conversationNode);
									haveMerged = true;
									break;
								}
							}
						}
						if(!haveMerged) conversation.StartNodes.Add(conversationNode);
					}
					else
					{
						if(ConversationNodeExtensions.LoadingConversationNodes.ContainsKey(conversationNode) && ConversationNodeExtensions.LoadingConversationNodes[conversationNode])
						{
							if(!conversation.NodesByID.ContainsKey(conversationNode.ID))
							{
								MetricsManager.LogError(string.Format("ConversationNode '{0}' not found in conversation '{1}', merge aborted!", conversationNode.ID, conversation.ID));
								MetricsManager.LogError(string.Format("Conversation '{0}' has nodes: {1}", conversation.ID, String.Join(", ", conversation.NodesByID.Keys)));
							}
							else
							{
								var canDoMerge = conversation.NodesByID[conversationNode.ID].canMerge(conversationNode);
								if(canDoMerge) conversation.NodesByID[conversationNode.ID].Merge(conversationNode);
							}
						}
						else
						{
							try
							{
								conversation.NodesByID.Add(conversationNode.ID, conversationNode);
							}
							catch (Exception ex)
							{
								MetricsManager.LogError("Duplicate node ID: " + conversationNode.ID);
								throw ex;
							}
						}
					}
				}
				if (Reader.NodeType == XmlNodeType.EndElement && Reader.Name == "conversation")
				{
					__result = conversation;
					return false;
				}
			}
			__result = conversation;
			return false;
		}
	}
}