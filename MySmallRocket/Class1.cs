using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using VRageMath;
using VRage.Game;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Ingame;
using Sandbox.Game.EntityComponents;
using VRage.Game.Components;
using VRage.Collections;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;
using VRage.Noise.Modifiers;

namespace Script1
{
    //Структуры, ибо классы нельзя
    public sealed class Program : MyGridProgram
    {
        //Начало скрипта
        IMyBlockGroup myBlockGroup;
        IMyTextPanel LCD;
        // Общие листы (не хотелось бы превратить корабаль с которого произведен запуск в ракету)
        List<IMyLargeTurretBase> guns;
        //Листы компонентов ракеты
        List<IMyShipMergeBlock> my_shipMB;
        List<IMyWarhead> myWarheads;
        List<IMyThrust> myThrusts;
        List<IMyGyro> myGyros;
        int count_fund;
        int count;
        long id;
        short rocket_active;
        short rocket_limit;
        string teg_grupp;
        public Program()
        {
            LCD = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel;

            count_fund = 0;
            count = 0;
            id = Me.CubeGrid.EntityId;
            rocket_limit = 4;
            teg_grupp = "MyRocket";

            guns = new List<IMyLargeTurretBase>();

            GridTerminalSystem.GetBlocksOfType<IMyLargeTurretBase>(guns);
            myBlockGroup = GridTerminalSystem.GetBlockGroupWithName(teg_grupp);
            my_shipMB = new List<IMyShipMergeBlock>();
            myWarheads = new List<IMyWarhead>();
            myGyros = new List<IMyGyro>();
            myThrusts = new List<IMyThrust>();

            myBlockGroup.GetBlocksOfType<IMyGyro>(myGyros);
            myBlockGroup.GetBlocksOfType<IMyThrust>(myThrusts);
            myBlockGroup.GetBlocksOfType<IMyWarhead>(myWarheads);
            myBlockGroup.GetBlocksOfType<IMyShipMergeBlock>(my_shipMB);

        }

        public void Main(string arg, UpdateType uType)
        {
            if (uType == UpdateType.Update1)
            {
                count++;
                LCD.WriteText($"Количество запусков программы: {count}");
                LCD.WriteText($"\nТурелли: {guns.Count} \n Гироскопы: {myGyros.Count} \n Двигатели: {myThrusts.Count} \n Боеголовки: {myWarheads.Count} \n Нужных блоков: {count_fund}", true);
                foreach (IMyLargeGatlingTurret gun in guns)
                {
                    if (gun.IsAimed == true)
                    {

                        TargetAttack(gun.GetTargetedEntity());
                        LCD.WriteText($"Цель в захвате: {gun.GetTargetedEntity().Name}", true);
                    }
                }
            }
            else
            {
                switch (arg)
                {
                    case "off":
                        Off_all(false);
                        Runtime.UpdateFrequency = UpdateFrequency.None;
                        break;
                    case "on":
                        Off_all(true);
                        Runtime.UpdateFrequency = UpdateFrequency.Update1;
                        break;
                    default:
                        break;

                }
            }
        }

        public void Save()
        { }
        // КОНЕЦ Скрипта
        public void TargetAttack(MyDetectedEntityInfo target)
        {
            Vector3D targetpos = target.Position;
            rocket_active = 0;
            foreach (IMyGyro mgr in myGyros)
            {
                if (mgr.CubeGrid.EntityId != id)
                {
                    rocket_active++;
                    SetGyro(Vector3D.Cross((targetpos - mgr.GetPosition()).Normalized(), mgr.WorldMatrix.Up), mgr);
                }
            }
            foreach (IMyThrust thr in myThrusts)
            {
                if (thr.CubeGrid.EntityId != id)
                {
                    thr.ThrustOverridePercentage = 1;
                }
            }
            foreach (IMyWarhead warhead in myWarheads)
            {
                if (warhead.CubeGrid.EntityId != id)
                {
                    warhead.IsArmed = true;
                }
            }
            if (rocket_active != rocket_limit)
            {
                int i = 0;
                foreach (IMyShipMergeBlock mrg in my_shipMB)
                {
                    if (i == rocket_limit)
                    {
                        break;
                    }
                    else if (mrg.CubeGrid.EntityId == id && mrg.IsConnected)
                    {
                        mrg.Enabled = false;
                        i++;
                    }

                }
            }
        } 
        public void SetGyro(Vector3D axis, IMyGyro gyro)
        {
            gyro.Yaw = (float)axis.Dot(gyro.WorldMatrix.Up);
            gyro.Pitch = (float)axis.Dot(gyro.WorldMatrix.Right);
            gyro.Roll = (float)axis.Dot(gyro.WorldMatrix.Backward);
        }

        public void Off_all(bool over)
        {
            foreach (IMyThrust tr in myThrusts)
            {
                tr.ThrustOverride = 0;
            }
            foreach (IMyGyro gr in myGyros)
            {
                gr.Yaw = 0;
                gr.Pitch = 0;
                gr.Roll = 0;
                gr.GyroOverride = over;
            }
            foreach (IMyWarhead warhead in myWarheads)
            {
                warhead.IsArmed = false;
            }
        }
    }
}