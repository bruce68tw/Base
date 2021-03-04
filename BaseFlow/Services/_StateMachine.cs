using BaseFlow.Enums;
using BaseFlow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaseFlow.Services
{
    public static class _StateMachine
    {
        public static StateMachineVM GetVM()
        {
            //return test data
            return new StateMachineVM()
            {
                Nodes = new List<NodeModel>()
                {
                    new NodeModel()
                    {
                        Id = "1",
                        Name = "請假申請",
                        X = 420,
                        Y = 20,
                    },
                    new NodeModel()
                    {
                        Id = "2",
                        Name = "主管審核",
                        X = 360,
                        Y = 146,
                    },
                    new NodeModel()
                    {
                        Id = "3",
                        Name = "總經理審核",
                        X = 560,
                        Y = 200,
                    },
                    new NodeModel()
                    {
                        Id = "4",
                        Name = "人事審核",
                        X = 435,
                        Y = 330,
                    },
                    new NodeModel()
                    {
                        Id = "5",
                        Name = "結束",
                        X = 448,
                        Y = 471,
                    },
                },
                Lines = new List<LineModel>()
                {
                    new LineModel()
                    {
                        //無條件送主管
                        FromNode = "1",
                        ToNode = "2",
                        //OrderNo = 1,
                        //Fid = "applyDays",
                        Op = EnumLineOp.Else,
                        //Value = "3"
                    },
                    /*
                    new LineModel()
                    {
                        //請假>=3天送總經理
                        FromSn = 2,
                        ToSn = 3,
                        OrderNo = 1,
                        Fid = "applyDays",
                        Op = EnumLineOp.Ge,
                        Value = "3"
                    },
                    new LineModel()
                    {
                        //主管 -> 人事
                        FromSn = 2,
                        ToSn = 4,
                        //OrderNo = 1,
                        //Fid = "applyDays",
                        Op = EnumLineOp.Else,
                        //Value = "3"
                    },
                    new LineModel()
                    {
                        //總經理 -> 人事
                        FromSn = 3,
                        ToSn = 4,
                        //OrderNo = 1,
                        //Fid = "applyDays",
                        Op = EnumLineOp.Else,
                        //Value = "3"
                    },
                    new LineModel()
                    {
                        //人事 -> 結束
                        FromSn = 4,
                        ToSn = 5,
                        //OrderNo = 1,
                        //Fid = "applyDays",
                        Op = EnumLineOp.Else,
                        //Value = "3"
                    },
                    */
                },
            };
        }

    } //class
} //namespace