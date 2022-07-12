using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
#if VRC_SDK_VRCSDK3
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using static VRC.SDK3.Avatars.Components.VRCAvatarDescriptor;
using static VRC.SDKBase.VRC_AvatarParameterDriver;
#endif

namespace Saturnian
{
    //定義用
    /// <summary>
    /// 定義用クラス
    /// </summary>
    public class ExpressionParam
    {
        string Name;
        bool isSaved;

        VRCExpressionParameters.ValueType Types;

        public ExpressionParam(string name, VRCExpressionParameters.ValueType type, bool issave = true)
        {
            Name = name;
            Types = type;
            isSaved = issave;
        }
        public VRCExpressionParameters.Parameter retParameter()
        {
            VRCExpressionParameters.Parameter param = new VRCExpressionParameters.Parameter();
            param.name = Name;
            param.valueType = Types;
            param.saved = isSaved;

            return param;
        }
    }
    /// <summary>
    /// 定義用
    /// </summary>
    public class ExpressionMenuControl
    {
        string Name;

        VRCExpressionsMenu.Control.ControlType Types;
        string Param;
        VRCExpressionsMenu Submenu;

        public ExpressionMenuControl(string name, VRCExpressionsMenu.Control.ControlType type, string param, VRCExpressionsMenu submenu = null)
        {
            Name = name;
            Types = type;
            Param = param;
            Submenu = submenu;
        }
        public VRCExpressionsMenu.Control retParameter()
        {
            var Control = new VRCExpressionsMenu.Control();
            Control.type = Types;
            Control.name = Name;
            Control.parameter = retControlParameter(Param);
            Control.subMenu = Submenu;

            return Control;
        }

        public VRCExpressionsMenu.Control.Parameter retControlParameter(string param)
        {
            var Parameter = new VRCExpressionsMenu.Control.Parameter();
            Parameter.name = param;

            return Parameter;
        }

    }
    //Functions
    public class AvatarDescriptorFunction
    {
        public static RuntimeAnimatorController retBaseAnimationController (VRCAvatarDescriptor _Avatar, AnimLayerType layerType)
        {
            if (!_Avatar.customExpressions)
                return null;

            int layerindex = GetLayerIndex(_Avatar, layerType);
            if (layerindex == -1)
                return null;

            return _Avatar.baseAnimationLayers[layerindex].animatorController;

        }
        public static int GetLayerIndex(VRCAvatarDescriptor _Avatar, AnimLayerType layerType)
        {

            if (_Avatar == null)
                return -1;

            if (!_Avatar.customizeAnimationLayers)
                return -1;

            for(int i = 0; i < _Avatar.baseAnimationLayers.Length; i++)
            {
                CustomAnimLayer animLayer = _Avatar.baseAnimationLayers[i];

                if (animLayer.type == layerType)
                    return i;
            }

            return -1;
        }
    }
    public class ExpressionFunction
    {
        public static bool ExpressionParamExists(VRCExpressionParameters param, string name)
        {
            return param.FindParameter(name) != null;
        }
        public static bool ExpressionParamAdd(VRCExpressionParameters param, VRCExpressionParameters.Parameter addParam)
        {
            if (ExpressionParamExists(param, addParam.name))
                ExpressionParamDelete(param, addParam.name);

            int index = 0;
            VRCExpressionParameters.Parameter[] Params = new VRCExpressionParameters.Parameter[param.parameters.Length + 1];
            foreach(var oldparam in param.parameters)
            {
                Params[index] = oldparam;
                index++;
            }
            Params[index] = addParam;

            param.parameters = Params;

            return true;
        }
        public static bool ExpressionParamDelete(VRCExpressionParameters param, string name)
        {
            if (!ExpressionParamExists(param, name))
                return true;

            int index = 0;
            VRCExpressionParameters.Parameter[] Params = new VRCExpressionParameters.Parameter[param.parameters.Length - 1];
            foreach (var oldparam in param.parameters)
            {
                if (oldparam.name != name)
                {
                    Params[index] = oldparam;
                    index++;
                }
            }

            param.parameters = Params;

            return true;
        }
        public static bool ExpressionMenuAdd(VRCExpressionsMenu menu, VRCExpressionsMenu.Control Control)
        {
            if (menu.controls.Count == 8)
                return false;

            if (ExpressionMenuExistsfromName(menu, Control.name))
            {
                ExpressionMenuDelete(menu, Control);
            }

            menu.controls.Add(Control);

            return true;
        }

        public static bool ExpressionMenuExistsfromName(VRCExpressionsMenu menu, string name)
        {
            foreach(var menus in menu.controls)
            {
                if (menus.name == name)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool ExpressionMenuDelete(VRCExpressionsMenu menu, VRCExpressionsMenu.Control Control)
        {
            if (ExpressionMenuExistsfromName(menu, Control.name))
            {
                menu.controls.Remove(Control);
                return true;
            }
            return false;
        }

    }
    public class RuntimeAnimatorFunction
    {
        public static void deleteLayer(AnimatorController FX, string name)
        {
            for (int i = 0; i < FX.layers.Length; i++)
            {
                if (FX.layers[i].name == name)
                {
                    FX.RemoveLayer(i);
                }
            }
        }
        /// <summary>
        /// oldversion 非推奨
        /// </summary>
        /// <param name="copyMoto"></param>
        /// <param name="copySaki"></param>
        public static void copyLayer(AnimatorController copyMoto, AnimatorController copySaki)
        {
            var motoLayers = copyMoto.layers;
            var sakiLayers = copySaki.layers;
            foreach (var tst in motoLayers)
            {
                tst.defaultWeight = 1.0f;
                copySaki.AddLayer(tst);
            }
        }
        public static void copyParameter(AnimatorController copyMoto, AnimatorController copySaki)
        {
            var animParam = copyMoto.parameters;

            foreach (var tst in animParam)
            {
                copySaki.AddParameter(tst);
            }
        }
        public static int retLayerIndex(AnimatorController controller, string name)
        {
            if (controller.layers.Length == 0)
                return -1;

            for (int i = 0; i < controller.layers.Length; i++)
            {
                var layer = controller.layers[i];

                if (layer.name == name)
                {
                    return i;
                }
            }

            return -1;
        }
        public static int retStateIndex(AnimatorControllerLayer layer, string name)
        {
            if (layer.stateMachine.states.Length == 0)
                return -1;

            for (int i = 0; i < layer.stateMachine.states.Length; i++)
            {
                if (layer.stateMachine.states[i].state.name == name)
                {
                    return i;
                }
            }

            return -1;
        }
        public static AnimatorState CreateState(string name)
        {
            AnimatorState _state = new AnimatorState();
            _state.name = name;

            return _state;
        }

        /// <summary>
        /// 新バージョン
        /// </summary>
        /// <param name="copyMoto"></param>
        /// <param name="copySaki"></param>
        public static void copyLayers(AnimatorController copyMoto, AnimatorController copySaki)
        {
            var copySakiControllerPath = AssetDatabase.GetAssetPath(copySaki);

            for (int i = 0; i < copyMoto.layers.Length; i++)
                addLayer(copySaki, copyMoto.layers[i], copySakiControllerPath);

            foreach (var param in copyMoto.parameters)
                addParameter(copySaki, param);

            EditorUtility.SetDirty(copySaki);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

        }
        public static AnimatorControllerLayer addLayer(AnimatorController controller, AnimatorControllerLayer copyMotoLayer, string controllerPath = "")
        {
            var newLayer = DuplicateLayer(copyMotoLayer, controller.MakeUniqueLayerName(copyMotoLayer.name));
            controller.AddLayer(newLayer);

            if (string.IsNullOrEmpty(controllerPath))
                controllerPath = AssetDatabase.GetAssetPath(controller);

            AddObjectsInStateMachineToAnimatorController(newLayer.stateMachine, controllerPath);

            return newLayer;
        }
        public static AnimatorControllerParameter addParameter(AnimatorController controller, AnimatorControllerParameter srcParameter)
        {
            if (controller.parameters.Any(p => p.name == srcParameter.name))
                return null;

            var parameter = new AnimatorControllerParameter
            {
                defaultBool = srcParameter.defaultBool,
                defaultFloat = srcParameter.defaultFloat,
                defaultInt = srcParameter.defaultInt,
                name = srcParameter.name,
                type = srcParameter.type
            };

            controller.AddParameter(parameter);
            return parameter;
        }
        private static AnimatorControllerLayer DuplicateLayer(AnimatorControllerLayer srcLayer, string dstLayerName)
        {
            var newLayer = new AnimatorControllerLayer()
            {
                avatarMask = srcLayer.avatarMask,
                blendingMode = srcLayer.blendingMode,
                defaultWeight = srcLayer.defaultWeight,
                iKPass = srcLayer.iKPass,
                name = dstLayerName,
                stateMachine = DuplicateStateMachine(srcLayer.stateMachine),
                syncedLayerAffectsTiming = srcLayer.syncedLayerAffectsTiming,
                syncedLayerIndex = srcLayer.syncedLayerIndex
            };

            newLayer.defaultWeight = 1f;

            CopyTransitions(srcLayer.stateMachine, newLayer.stateMachine);

            return newLayer;
        }

        private static AnimatorStateMachine DuplicateStateMachine(AnimatorStateMachine srcStateMachine)
        {
            var dstStateMachine = new AnimatorStateMachine
            {
                anyStatePosition = srcStateMachine.anyStatePosition,
                entryPosition = srcStateMachine.entryPosition,
                exitPosition = srcStateMachine.exitPosition,
                hideFlags = srcStateMachine.hideFlags,
                name = srcStateMachine.name,
                parentStateMachinePosition = srcStateMachine.parentStateMachinePosition,
                stateMachines = srcStateMachine.stateMachines
                                    .Select(cs =>
                                        new ChildAnimatorStateMachine
                                        {
                                            position = cs.position,
                                            stateMachine = DuplicateStateMachine(cs.stateMachine)
                                        })
                                    .ToArray(),
                states = DuplicateChildStates(srcStateMachine.states),
            };

            foreach (var srcBehaivour in srcStateMachine.behaviours)
            {
                var behaivour = dstStateMachine.AddStateMachineBehaviour(srcBehaivour.GetType());
                CopyBehaivourParameters(srcBehaivour, behaivour);
            }

            if (srcStateMachine.defaultState != null)
            {
                var defaultStateIndex = srcStateMachine.states
                                    .Select((value, index) => new { Value = value.state, Index = index })
                                    .Where(s => s.Value == srcStateMachine.defaultState)
                                    .Select(s => s.Index).SingleOrDefault();
                dstStateMachine.defaultState = dstStateMachine.states[defaultStateIndex].state;
            }

            return dstStateMachine;
        }

        private static ChildAnimatorState[] DuplicateChildStates(ChildAnimatorState[] srcChildStates)
        {
            var dstStates = new ChildAnimatorState[srcChildStates.Length];

            for (int i = 0; i < srcChildStates.Length; i++)
            {
                var srcState = srcChildStates[i].state;
                dstStates[i] = new ChildAnimatorState
                {
                    position = srcChildStates[i].position,
                    state = DuplicateAnimatorState(srcState)
                };

                foreach (var srcBehaivour in srcChildStates[i].state.behaviours)
                {
                    var behaivour = dstStates[i].state.AddStateMachineBehaviour(srcBehaivour.GetType());
                    CopyBehaivourParameters(srcBehaivour, behaivour);
                }
            }

            return dstStates;
        }

        private static AnimatorState DuplicateAnimatorState(AnimatorState srcState)
        {
            return new AnimatorState
            {
                cycleOffset = srcState.cycleOffset,
                cycleOffsetParameter = srcState.cycleOffsetParameter,
                cycleOffsetParameterActive = srcState.cycleOffsetParameterActive,
                hideFlags = srcState.hideFlags,
                iKOnFeet = srcState.iKOnFeet,
                mirror = srcState.mirror,
                mirrorParameter = srcState.mirrorParameter,
                mirrorParameterActive = srcState.mirrorParameterActive,
                motion = srcState.motion,
                name = srcState.name,
                speed = srcState.speed,
                speedParameter = srcState.speedParameter,
                speedParameterActive = srcState.speedParameterActive,
                tag = srcState.tag,
                timeParameter = srcState.timeParameter,
                timeParameterActive = srcState.timeParameterActive,
                writeDefaultValues = srcState.writeDefaultValues
            };
        }

        private static void CopyTransitions(AnimatorStateMachine srcStateMachine, AnimatorStateMachine dstStateMachine)
        {
            var srcStates = GetAllStates(srcStateMachine);
            var dstStates = GetAllStates(dstStateMachine);
            var srcStateMachines = GetAllStateMachines(srcStateMachine);
            var dstStateMachines = GetAllStateMachines(dstStateMachine);

            for (int i = 0; i < srcStates.Length; i++)
            {
                foreach (var srcTransition in srcStates[i].transitions)
                {
                    AnimatorStateTransition dstTransition;

                    if (srcTransition.isExit)
                    {
                        dstTransition = dstStates[i].AddExitTransition();
                    }
                    else if (srcTransition.destinationState != null)
                    {
                        var stateIndex = Array.IndexOf(srcStates, srcTransition.destinationState);
                        dstTransition = dstStates[i].AddTransition(dstStates[stateIndex]);
                    }
                    else if (srcTransition.destinationStateMachine != null)
                    {
                        var stateMachineIndex = Array.IndexOf(srcStateMachines, srcTransition.destinationStateMachine);
                        dstTransition = dstStates[i].AddTransition(dstStateMachines[stateMachineIndex]);
                    }
                    else continue;

                    CopyTransitionParameters(srcTransition, dstTransition);
                }
            }

            for (int i = 0; i < srcStateMachines.Length; i++)
            {
                CopyTransitionOfSubStateMachine(srcStateMachines[i], dstStateMachines[i],
                                                srcStates, dstStates,
                                                srcStateMachines, dstStateMachines);

                foreach (var srcTransition in srcStateMachines[i].anyStateTransitions)
                {
                    AnimatorStateTransition dstTransition;

                    if (srcTransition.isExit)
                    {
                        Debug.LogError($"Unknown transition:{srcStateMachines[i].name}.AnyState->Exit");
                        continue;
                    }
                    else if (srcTransition.destinationState != null)
                    {
                        var stateIndex = Array.IndexOf(srcStates, srcTransition.destinationState);
                        dstTransition = dstStateMachines[i].AddAnyStateTransition(dstStates[stateIndex]);
                    }
                    else if (srcTransition.destinationStateMachine != null)
                    {
                        var stateMachineIndex = Array.IndexOf(srcStateMachines, srcTransition.destinationStateMachine);
                        dstTransition = dstStateMachines[i].AddAnyStateTransition(dstStateMachines[stateMachineIndex]);
                    }
                    else continue;

                    CopyTransitionParameters(srcTransition, dstTransition);
                }

                foreach (var srcTransition in srcStateMachines[i].entryTransitions)
                {
                    AnimatorTransition dstTransition;

                    if (srcTransition.isExit)
                    {
                        Debug.LogError($"Unknown transition:{srcStateMachines[i].name}.Entry->Exit");
                        continue;
                    }
                    else if (srcTransition.destinationState != null)
                    {
                        var stateIndex = Array.IndexOf(srcStates, srcTransition.destinationState);
                        dstTransition = dstStateMachines[i].AddEntryTransition(dstStates[stateIndex]);
                    }
                    else if (srcTransition.destinationStateMachine != null)
                    {
                        var stateMachineIndex = Array.IndexOf(srcStateMachines, srcTransition.destinationStateMachine);
                        dstTransition = dstStateMachines[i].AddEntryTransition(dstStateMachines[stateMachineIndex]);
                    }
                    else continue;

                    CopyTransitionParameters(srcTransition, dstTransition);
                }
            }

        }

        private static void CopyTransitionOfSubStateMachine(AnimatorStateMachine srcParentStateMachine, AnimatorStateMachine dstParentStateMachine,
                                                     AnimatorState[] srcStates, AnimatorState[] dstStates,
                                                     AnimatorStateMachine[] srcStateMachines, AnimatorStateMachine[] dstStateMachines)
        {
            for (int i = 0; i < srcParentStateMachine.stateMachines.Length; i++)
            {
                var srcSubStateMachine = srcParentStateMachine.stateMachines[i].stateMachine;
                var dstSubStateMachine = dstParentStateMachine.stateMachines[i].stateMachine;

                foreach (var srcTransition in srcParentStateMachine.GetStateMachineTransitions(srcSubStateMachine))
                {
                    AnimatorTransition dstTransition;

                    if (srcTransition.isExit)
                    {
                        dstTransition = dstParentStateMachine.AddStateMachineExitTransition(dstSubStateMachine);
                    }
                    else if (srcTransition.destinationState != null)
                    {
                        var stateIndex = Array.IndexOf(srcStates, srcTransition.destinationState);
                        dstTransition = dstParentStateMachine.AddStateMachineTransition(dstSubStateMachine, dstStates[stateIndex]);
                    }
                    else if (srcTransition.destinationStateMachine != null)
                    {
                        var stateMachineIndex = Array.IndexOf(srcStateMachines, srcTransition.destinationStateMachine);
                        dstTransition = dstParentStateMachine.AddStateMachineTransition(dstSubStateMachine, dstStateMachines[stateMachineIndex]);
                    }
                    else continue;

                    CopyTransitionParameters(srcTransition, dstTransition);
                }
            }
        }

        private static AnimatorState[] GetAllStates(AnimatorStateMachine stateMachine)
        {
            var stateList = stateMachine.states.Select(sc => sc.state).ToList();
            foreach (var subStatetMachine in stateMachine.stateMachines)
            {
                stateList.AddRange(GetAllStates(subStatetMachine.stateMachine));
            }
            return stateList.ToArray();
        }

        private static AnimatorStateMachine[] GetAllStateMachines(AnimatorStateMachine stateMachine)
        {
            var stateMachineList = new List<AnimatorStateMachine>
            {
                stateMachine
            };

            foreach (var subStateMachine in stateMachine.stateMachines)
            {
                stateMachineList.AddRange(GetAllStateMachines(subStateMachine.stateMachine));
            }

            return stateMachineList.ToArray();
        }

        private static void CopyTransitionParameters(AnimatorStateTransition srcTransition, AnimatorStateTransition dstTransition)
        {
            dstTransition.canTransitionToSelf = srcTransition.canTransitionToSelf;
            dstTransition.duration = srcTransition.duration;
            dstTransition.exitTime = srcTransition.exitTime;
            dstTransition.hasExitTime = srcTransition.hasExitTime;
            dstTransition.hasFixedDuration = srcTransition.hasFixedDuration;
            dstTransition.hideFlags = srcTransition.hideFlags;
            dstTransition.isExit = srcTransition.isExit;
            dstTransition.mute = srcTransition.mute;
            dstTransition.name = srcTransition.name;
            dstTransition.offset = srcTransition.offset;
            dstTransition.interruptionSource = srcTransition.interruptionSource;
            dstTransition.orderedInterruption = srcTransition.orderedInterruption;
            dstTransition.solo = srcTransition.solo;
            foreach (var srcCondition in srcTransition.conditions)
            {
                dstTransition.AddCondition(srcCondition.mode, srcCondition.threshold, srcCondition.parameter);
            }
        }

        private static void CopyTransitionParameters(AnimatorTransition srcTransition, AnimatorTransition dstTransition)
        {
            dstTransition.hideFlags = srcTransition.hideFlags;
            dstTransition.isExit = srcTransition.isExit;
            dstTransition.mute = srcTransition.mute;
            dstTransition.name = srcTransition.name;
            dstTransition.solo = srcTransition.solo;
            foreach (var srcCondition in srcTransition.conditions)
            {
                dstTransition.AddCondition(srcCondition.mode, srcCondition.threshold, srcCondition.parameter);
            }

        }

        private static void CopyBehaivourParameters(StateMachineBehaviour srcBehaivour, StateMachineBehaviour dstBehaivour)
        {
            if (srcBehaivour.GetType() != dstBehaivour.GetType())
            {
                throw new ArgumentException("Should be same type");
            }
#if VRC_SDK_VRCSDK3

            if (dstBehaivour is VRCAnimatorLayerControl layerControl)
            {
                var srcControl = srcBehaivour as VRCAnimatorLayerControl;
                layerControl.ApplySettings = srcControl.ApplySettings;
                layerControl.blendDuration = srcControl.blendDuration;
                layerControl.debugString = srcControl.debugString;
                layerControl.goalWeight = srcControl.goalWeight;
                layerControl.layer = srcControl.layer;
                layerControl.playable = srcControl.playable;
            }
            else if (dstBehaivour is VRCAnimatorLocomotionControl locomotionControl)
            {
                var srcControl = srcBehaivour as VRCAnimatorLocomotionControl;
                locomotionControl.ApplySettings = srcControl.ApplySettings;
                locomotionControl.debugString = srcControl.debugString;
                locomotionControl.disableLocomotion = srcControl.disableLocomotion;
            }

            else if (dstBehaivour is VRCAnimatorTemporaryPoseSpace poseSpace)
            {
                var srcPoseSpace = srcBehaivour as VRCAnimatorTemporaryPoseSpace;
                poseSpace.ApplySettings = srcPoseSpace.ApplySettings;
                poseSpace.debugString = srcPoseSpace.debugString;
                poseSpace.delayTime = srcPoseSpace.delayTime;
                poseSpace.enterPoseSpace = srcPoseSpace.enterPoseSpace;
                poseSpace.fixedDelay = srcPoseSpace.fixedDelay;
            }
            else if (dstBehaivour is VRCAnimatorTrackingControl trackingControl)
            {
                var srcControl = srcBehaivour as VRCAnimatorTrackingControl;
                trackingControl.ApplySettings = srcControl.ApplySettings;
                trackingControl.debugString = srcControl.debugString;
                trackingControl.trackingEyes = srcControl.trackingEyes;
                trackingControl.trackingHead = srcControl.trackingHead;
                trackingControl.trackingHip = srcControl.trackingHip;
                trackingControl.trackingLeftFingers = srcControl.trackingLeftFingers;
                trackingControl.trackingLeftFoot = srcControl.trackingLeftFoot;
                trackingControl.trackingLeftHand = srcControl.trackingLeftHand;
                trackingControl.trackingMouth = srcControl.trackingMouth;
                trackingControl.trackingRightFingers = srcControl.trackingRightFingers;
                trackingControl.trackingRightFoot = srcControl.trackingRightFoot;
                trackingControl.trackingRightHand = srcControl.trackingRightHand;
            }
            else if (dstBehaivour is VRCAvatarParameterDriver parameterDriver)
            {
                var srcDriver = srcBehaivour as VRCAvatarParameterDriver;
                parameterDriver.localOnly = srcDriver.localOnly;
                parameterDriver.debugString = srcDriver.debugString;
                parameterDriver.parameters = srcDriver.parameters
                                                .Select(p =>
                                                new Parameter
                                                {
                                                    name = p.name,
                                                    value = p.value
                                                })
                                                .ToList();
            }
            else if (dstBehaivour is VRCPlayableLayerControl playableLayerControl)
            {
                var srcControl = srcBehaivour as VRCPlayableLayerControl;
                playableLayerControl.ApplySettings = srcControl.ApplySettings;
                playableLayerControl.blendDuration = srcControl.blendDuration;
                playableLayerControl.debugString = srcControl.debugString;
                playableLayerControl.goalWeight = srcControl.goalWeight;
                playableLayerControl.layer = srcControl.layer;
                playableLayerControl.outputParamHash = srcControl.outputParamHash;
            }
#endif
        }

        private static void AddObjectsInStateMachineToAnimatorController(AnimatorStateMachine stateMachine, string controllerPath)
        {
            AssetDatabase.AddObjectToAsset(stateMachine, controllerPath);
            foreach (var childState in stateMachine.states)
            {
                AssetDatabase.AddObjectToAsset(childState.state, controllerPath);
                foreach (var transition in childState.state.transitions)
                {
                    AssetDatabase.AddObjectToAsset(transition, controllerPath);
                }
                foreach (var behaviour in childState.state.behaviours)
                {
                    AssetDatabase.AddObjectToAsset(behaviour, controllerPath);
                }
            }
            foreach (var transition in stateMachine.anyStateTransitions)
            {
                AssetDatabase.AddObjectToAsset(transition, controllerPath);
            }
            foreach (var transition in stateMachine.entryTransitions)
            {
                AssetDatabase.AddObjectToAsset(transition, controllerPath);
            }
            foreach (var behaviour in stateMachine.behaviours)
            {
                AssetDatabase.AddObjectToAsset(behaviour, controllerPath);
            }
            foreach (var SubStateMachine in stateMachine.stateMachines)
            {
                foreach (var transition in stateMachine.GetStateMachineTransitions(SubStateMachine.stateMachine))
                {
                    AssetDatabase.AddObjectToAsset(transition, controllerPath);
                }
                AddObjectsInStateMachineToAnimatorController(SubStateMachine.stateMachine, controllerPath);
            }
        }

        public static AnimatorController DuplicateAnimationLayerController(string originalControllerPath, string outputFolderPath, string avatarName)
        {
            var controllerName = $"{Path.GetFileNameWithoutExtension(originalControllerPath)}_{avatarName}.controller";
            var controllerPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(outputFolderPath, controllerName));
            AssetDatabase.CopyAsset(originalControllerPath, controllerPath);
            return AssetDatabase.LoadAssetAtPath(controllerPath, typeof(AnimatorController)) as AnimatorController;
        }
    }
    public class GameObjectFunction
    {        
        /// <summary>
        /// 指定されたオブジェクトがプレハブの場合 true を返します
        /// </summary>
        public static bool IsPrefab(UnityEngine.GameObject self)
        {
            var type = PrefabUtility.GetPrefabAssetType(self);

            return
                type == PrefabAssetType.Model ||
                type == PrefabAssetType.MissingAsset ||
                type == PrefabAssetType.Regular ||
                type == PrefabAssetType.Variant;
        }
        public static void BackupObject(GameObject Object)
        {
            GameObject Backup = UnityEngine.Object.Instantiate(Object.gameObject);
            Backup.name = Object.name + "- Backup";
            Backup.SetActive(false);
        }
        public static void GetAllChildren(GameObject obj, out List<Transform> lists)
        {
            Transform children = obj.GetComponentInChildren<Transform>();
            lists = new List<Transform>();
            //子要素がいなければ終了
            if (children.childCount == 0)
            {
                return;
            }
            foreach (Transform ob in children)
            {
                lists.Add(ob);
                GetAllChildren(ob.gameObject, out lists);
            }
        }
        
    }
    public class SaturnianFunction
    {
        public static GameObject CreateSaturnianObject()
        {
            GameObject SaturnianObj = GameObject.Find("_Saturnian");
            if (SaturnianObj == null)
                SaturnianObj = new GameObject("_Saturnian");

            return SaturnianObj;
        }

        public static void DeleteSaturnianObject()
        {
            GameObject SaturnianObj = GameObject.Find("_Saturnian");
            if (SaturnianObj != null)
            {
                GameObject.DestroyImmediate(SaturnianObj);
            }
        }
    }
    public class BoneFunction
    {
        public static Transform forloopfinding(HumanBodyBones b, Transform parent, string matchPattern, string word = "")
        {
            foreach (Transform childbone in parent)
            {
                if (Regex.IsMatch(childbone.name, matchPattern, RegexOptions.IgnoreCase))
                {
                    if (word != "")
                    {
                        if (!Regex.IsMatch(childbone.name, word, RegexOptions.IgnoreCase))
                        {
                            if (childbone.childCount > 0)
                            {
                                Transform temp = forloopfinding(b, childbone, matchPattern, word);
                                if (temp != null)
                                    return temp;
                            }
                        }
                    }
                    return childbone;
                }
            }
            return null;
        }
        public static Transform FindBone(HumanBodyBones b, Transform parent, string matchPattern, string word = "")
        {
            if (parent == null)
                return null;

            return forloopfinding(b, parent, matchPattern, word);
        }
        public static Dictionary<HumanBodyBones, Transform> GetBonesFromAnimator(Animator _humanoidanimator)
        {
            if (!_humanoidanimator.isHuman)
                return null;

            Dictionary<HumanBodyBones, Transform> bones = new Dictionary<HumanBodyBones, Transform>();

            for (int i = (int)HumanBodyBones.Hips; i < (int)HumanBodyBones.LastBone; i++)
            {
                bones[(HumanBodyBones)i] = _humanoidanimator.GetBoneTransform((HumanBodyBones)i);
            }

            return bones;
        }
    }
    public class LanguageOption<T>
    {
        public List<T> readLanguageJsons(string directoryPath, out string[] languageEnums)
        {
            int index = 0;
            languageEnums = new string[5];

            List<T> retTexts = new List<T>();
            if (!Directory.Exists(directoryPath))
                return null;

            string[] filePathArray = Directory.GetFiles(directoryPath, "*.json", SearchOption.TopDirectoryOnly);
            foreach (string filePath in filePathArray)
            {
                TextAsset languageFile = AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
                T tempTexts = JsonUtility.FromJson<T>(languageFile.text);
                languageEnums[index] = Path.GetFileName(filePath);
                retTexts.Add(tempTexts);

                index++;
            }
            return retTexts;
        }
    }
    public class CameraFunction
    {
        string Name;
        GameObject SaturnianObj;
        GameObject CamObj;
        Camera Cam;

        public CameraFunction(string name, Vector3 position, Quaternion quate)
        {
            SaturnianObj = SaturnianFunction.CreateSaturnianObject();

            Name = name;

            CamObj = new GameObject(name, typeof(Camera));
            CamObj.transform.SetParent(SaturnianObj.transform);
            CamObj.transform.SetPositionAndRotation(position, quate);

            Cam = CamObj.GetComponent<Camera>();
        }

        public GameObject retObject()
        {
            return CamObj;
        }

        public Camera retCamera()
        {
            return Cam;
        }
    }
    public class MonitorUI
    {
        Material _mat;
        private RenderTexture renderTexture;
        Rect rect;
        Camera Cam;

        private Material CreateGammaMaterial()
        {
            Shader gammaShader = Shader.Find("Saturnian/Unlit/Gamma_Anlabo");
            return new Material(gammaShader);
        }
        public MonitorUI(Camera cam, Rect _rect, int _sizeX = 300, int _sizeY = 300)
        {
            _mat = CreateGammaMaterial();
            rect = _rect;
            Cam = cam;
            renderTexture = new RenderTexture(_sizeX, _sizeY, 64);
            Cam.targetTexture = renderTexture;
        }
        public void ChangeCamera(Camera cam)
        {
            cam.targetTexture = renderTexture;
        }
        public void DrawGUI(int _sizeX = 300, int _sizeY = 300)
        {
            Cam.targetTexture = renderTexture;
            
            Graphics.DrawTexture(rect, renderTexture, _mat);
            EditorUtility.SetDirty(renderTexture);
        }
    }
    public class EditorLayoutFunction
    {
        public static bool isProTheme()
        {
            return EditorGUIUtility.isProSkin;
        }
    }
}
