using System.Collections.Generic;
using System.Numerics;
using Unity.Mathematics;

namespace ET.Server
{
    public static class MoveHelper
    {
        // 可以多次调用，多次调用的话会取消上一次的协程
        public static async ETTask FindPathMoveToAsync(this Unit unit, float3 target, ETCancellationToken cancellationToken = null)
        {
            float speed = unit.GetComponent<NumericComponent>().GetAsFloat(NumericType.Speed);
            if (speed < 0.01)
            {
                unit.SendStop(-1);
                return;
            }

            List<float3> list = new List<float3>();
            
            unit.GetComponent<PathfindingComponent>().Find(unit.Position, target, list);

            if (list.Count < 2)
            {
                unit.SendStop(0);
                return;
            }
                
            // 广播寻路路径
            M2C_PathfindingResult m2CPathfindingResult = new M2C_PathfindingResult() {Points = new List<float3>()};
            m2CPathfindingResult.Id = unit.Id;
            for (int i = 0; i < list.Count; ++i)
            {
                m2CPathfindingResult.Points = list;
            }
            MessageHelper.Broadcast(unit, m2CPathfindingResult);

            bool ret = await unit.GetComponent<MoveComponent>().MoveToAsync(list, speed);
            if (ret) // 如果返回false，说明被其它移动取消了，这时候不需要通知客户端stop
            {
                unit.SendStop(0);
            }
        }

        public static void Stop(this Unit unit, int error)
        {
            unit.GetComponent<MoveComponent>().Stop();
            unit.SendStop(error);
        }

        public static void SendStop(this Unit unit, int error)
        {
            MessageHelper.Broadcast(unit, new M2C_Stop()
            {
                Error = error,
                Id = unit.Id, 
                Position = unit.Position,
                Rotation = unit.Rotation,
            });
        }
    }
}