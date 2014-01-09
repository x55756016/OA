/// <reference path="/Scripts/jquery-1.7.min.js" />

////2014年只有深圳号码，去掉号码之前SZ和BJ字样，并注释掉生成北京号码的代码


function AwardEntity(ticketNO, level, remark) {
    this.TicketNO = ticketNO;
    this.Level = level;
    this.Remark = remark;
}

function Lucky() {

    this.Ticket = new Array();
    this.Award = new Array();

    this.PublishBatch = "";
    this.Init = function (tickets, awardExisted) {
        this.Ticket = tickets;
        this.Award = awardExisted;
    }

    this.DiscardAward = function (ticketNO) {
        var index = null;
        for (var i = 0; i < this.Award.length; i++) {
            if (this.Award[i].TicketNO == ticketNO) {
                index = i;
                break;
            }
        }
        this.Award.splice(index, 1);
    }


     this.szCount = new Array();
     //this.bjCount = new Array();
     var number = new Array();

        ///产生号码,产生号码都是从这个方法开始 qty表示产生号码个数（三等奖为产生一个数字）
     this.GenerateAward = function (level, qty) {
         var result = new Array();
         this.getArray();
         if (level == 0 || level == 1 || level == 2) {
             var ro = this.GetTheAward(level);
             if (ro != "0000") {
                 result = ro;
             }
         }
         else if (level == 3) {
             result = this.GenerateAwardThird(level, qty);
         }
         else {
             result[result.length] = this.generateOneAward(level); //要是传进来一个没等级的值那么就随便抽一个
         }
         return result;
     }

     //产生特，一，二等奖号（三等奖只有一个号码）
     this.GetTheAward = function (level) {
         var result = new Array();
         var awardExisted = dbProxy.GetAwardObj(); //获取所有中奖的号码
         var awardLevel1 = new Array();
         var countSZ1 = 0; //一等奖6名
         var countSZ2 = 0; //一等奖20名
         for (var i = 0; i < awardExisted.length; i++) {
             if (awardExisted[i].Level == "1") {
                 countSZ1++;
             }
             if (awardExisted[i].Level == "2") {
                 countSZ2++;
             }
         }
         if (level == 1 && countSZ1 >= 6) {//如果是一等奖大于6个（一等奖只有6个）则返回，如果添加不限制，去掉判断即可
             return "0000";
         }
         if (level == 2 && countSZ2 >= 20) {//如果是一等奖大于20个（二等奖只有20个）则返回，如果添加不限制，去掉判断即可
             return "0000";
         }
         if (countSZ1 == 5 || countSZ2 == 19 || level == 0) {
             return result[result.length] = this.generateOneAward(level); //产生一个号码;
         }
         else {
             for (var i = 0; i < 2; i++) {
                 result[result.length] = this.generateOneAward(level);
             }
             return result;
         }
     }


     ///生成中奖号码，从全部没有中奖的号码中进行抽取
     this.generateOneAward = function (level) {
         debugger;
         var index = this.getRandom(this.Ticket.length);
         var ticketNO = this.Ticket[index];
         var award = new AwardEntity(ticketNO, level, this.PublishBatch);
         this.updateCandidate(index);
         this.updateAward(award);
         return award;
     }


     ///为了三等奖
     var szcount = 1;
     //var bjcount = 1;
     var randomCountArray = new Array();
     this.GenerateAwardThird = function (level, number) {
         var result = new Array(); //存总共信息（有总中奖人员号码，两地中奖人数及产生的中奖号码）
         var szArray = new Array(); //存深圳号码信息
         // var bjArray = new Array(); //存北京号码信息
         randomCount = this.randomCount();
         var indexsz = randomCount; //加入号码为3，那么3,13,23,33...后面有3的都中奖
         //var indexbj = randomCount;
         for (var i = 0; i < this.Ticket.length; i++) {
             var szStr = this.Ticket[i][this.Ticket[i].length - 1];
             if (randomCount == szStr && szcount < 1001) {
                 szArray.push(this.updateTicketNO(this.Ticket[i], level));
                 szcount++;
             }
         }
         result.push(randomCount);
         result.push(szcount - 1); //深圳人数
         // result.push(bjcount - 1); //北京人数
         result.push(szArray); //深圳号码
         //result.push(bjArray); //北京号码
         return result;
     }

     //对中奖号码进行处理
     this.updateTicketNO = function (ticketNO, level) {
         var award = new AwardEntity(ticketNO, level, this.PublishBatch);
         //this.updateCandidate(index);
         this.updateCandidateTicketNO(ticketNO);
         this.updateAward(award);
         return award;
     }

     //产生随机数
     this.randomCount = function () {
         randomCount = Math.floor(Math.random() * 9) + 0;
         if (!this.IsSame(randomCount, randomCountArray)) {
             randomCountArray.push(randomCount); //存入到数组里面，避免两次一样数字
         }
         else {
             this.randomCount();
         }
         return randomCount;
     }

     //判断一个数是否在这这个数组里面,有相同的返回true
     this.IsSame = function (randomCount, randomCountArray) {
         var flag = false;
         for (var i = 0; i < randomCountArray.length; i++) {
             if (randomCount == randomCountArray[i]) {
                 flag = true;
                 break;
             }
         }
         return flag;
     }


     ///生成深圳号码（根据传入的index）（三等奖）
     this.getOneAwardSZ = function (index, level) {
         if (index < 0) {
             index = 9; //如果index为0那么有效数字是this.szCount[9]=sz_0010，
         } 
         var ticketNO = this.szCount[index];
         var award = new AwardEntity(ticketNO, level, this.PublishBatch);
         //this.updateCandidate(index);
         this.updateCandidateTicketNO(ticketNO);
         this.updateAward(award);
         return award;
     }

     ///生成北京号码（根据传入的index）（三等奖）
     this.getOneAwardBJ = function (index, level) {
         if (index < 0) {
             index = 9;//如果是0，那么从数组的9开始末尾是0的数字才有效
         }
         var ticketNO = this.bjCount[index];
         var award = new AwardEntity(ticketNO, level, this.PublishBatch);
         //this.updateCandidate(index);
         this.updateCandidateTicketNO(ticketNO);
         this.updateAward(award);
         return award;
     }

    //从一个数组里面分别存入深圳和北京的数组
     this.getArray = function () {
         this.szCount = this.Ticket;
     }

     //三等奖加载，loadData也会加载，但是要去掉中奖号码，所以新写一个
     this.getArrayNew = function () {
         var data = new Array();
         //生成所有抽奖号码
         for (var i = 1; i < szCount; i++) {
             var paded = "";
             if (i < 10) paded = "000" + i.toString();
             else if (i < 100) paded = "00" + i.toString();
             else if (i < 1000) paded = "0" + i.toString();
             // paded = "SZ-" + paded;
             paded = paded;
             data[data.length] = paded; //Lucky.js
         }
         var maxIndex = 1000;
         this.szCount = data;
     }

    //分别调用相应函数产生中奖号码
    this.forCount = function (level, sz, bj) {
        var awardArray = new Array();
        for (var i = 0; i < sz; i++) {
            var award = this.generateOneAwardSZ(level);
            awardArray.push(award);
        }
        for (var i = 0; i < bj; i++) {
            var award = this.generateOneAwardBJ(level);
            awardArray.push(award);
        }
        return awardArray;
    }



    ///生成深圳号码
    this.generateOneAwardSZ = function (level) {
        var index = this.getRandom(this.szCount.length);
        var ticketNO = this.szCount[index];
        var award = new AwardEntity(ticketNO, level, this.PublishBatch);
        //this.updateCandidate(index);
        this.updateCandidateTicketNO(ticketNO);

        this.updateAward(award);
        return award;
    }
    ///生成北京号码
    this.generateOneAwardBJ = function (level) {
        var index = this.getRandom(this.bjCount.length);
        var ticketNO = this.bjCount[index];
        var award = new AwardEntity(ticketNO, level, this.PublishBatch);
        //this.updateCandidate(index);
        this.updateCandidateTicketNO(ticketNO);
        this.updateAward(award);
        return award;
    }

    

    


    this.getRandom = function (max) {
        return Math.floor(Math.random() * (max - 1)) + 1;
        
    }

    //根据数组索引剔除
    this.updateCandidate = function (index) {
        this.Ticket.splice(index, 1); //从总数中剔除产生的号码

    }
    //根据号码剔除
    this.updateCandidateTicketNO = function (ticketNO) {
        var index = 0;
        for (var i = 0; i < this.Ticket.length; i++) {
            if (this.Ticket[i].indexOf(ticketNO) == 0) {
                index = i;
            }
        }
        this.Ticket.splice(index, 1); //从总数中剔除产生的号码

    }


    this.updateAward = function (award) {
        this.Award[this.Award.length] = award;
    }
    

    //特等奖深圳号码
    this.GenerateAward0SZ = function () {
//        var maxIndex4SZ = 0;
//        for (var i = 0; i < this.Ticket.length; i++) {
//            if (this.Ticket[i].indexOf('B') == 0) {
//                maxIndex4SZ = i - 1;
//                break;
//            }
//        }

        var index = this.getRandom(this.Ticket.length);
        var ticketNO = this.Ticket[index];
        var award = new AwardEntity(ticketNO, 0, "");

        this.updateCandidate(index);
        this.updateAward(award);
        return award;
    }

    //特等奖北京号码
    this.GenerateAward0BJ = function () {
//        var maxIndex4SZ = 0; 
//        for (var i = 0; i < this.Ticket.length; i++) {
//            if (this.Ticket[i].indexOf('B') == 0) {
//                maxIndex4SZ = i - 1;
//                break;
//            }
//        }
        //生成中奖号码
        var index = this.getRandom(this.Ticket.length);
        var ticketNO = this.Ticket[index];
        var award = new AwardEntity(ticketNO, 0, "");

        this.updateCandidate(index); //Lucky.js 把中奖号从全局的所有抽奖号数组中删除掉
        this.updateAward(award); //Lucky.js 将中奖号保存在全局已中奖数组中？
        return award;
    }

    ///获取中奖结果,szOrbj号码前面前缀，level几等奖
    this.GetAwardBy = function (szOrbj, level) {
        var result = new Array();
        var awardExisted = dbProxy.GetAwardObj(); //获取所有中奖的号码
        for (var i = 0; i < awardExisted.length; i++) {
            //            if (this.Award[i].TicketNO.substr(0, 2) == szOrbj && this.Award[i].Level == level)
            //                result[result.length] = this.Award[i].TicketNO;
            if (this.Award[i].Level == level)
                result[result.length] = awardExisted[i].TicketNO;
        }
        return result;
    }

    this.GetAwardByBatch = function (szOrbj, batch) {
        var result = new Array();
        for (var i = 0; i < this.Award.length; i++) {
            if (this.Award[i].TicketNO.substr(0, 2) == szOrbj && this.Award[i].Level == 3 && this.Award[i].Remark == batch)
             result[result.length] = this.Award[i].TicketNO;
        }
        return result;
    }
    ///GetAwardBy什么什么这几方法从数据库中取出中奖号码信息,这个给二等奖用，没有考虑扩展性，不过可以改为通用
    this.GetAwardByBatchLevel = function (szOrbj, batch) {
        var result = new Array();
        for (var i = 0; i < this.Award.length; i++) {
            if (this.Award[i].TicketNO.substr(0, 2) == szOrbj && this.Award[i].Level == 2 && this.Award[i].Remark == batch)
             result[result.length] = this.Award[i].TicketNO;
        }
        return result;
    }

    //该号码有中奖记录则返回false
    this.IsCandidate = function (ticketNO, awardExisted) {
        for (var i = 0; i < awardExisted.length; i++) {
            if (awardExisted[i].TicketNO == ticketNO) return false;
        }
        return true;
    }

}

var lucky = new Lucky();
var szCount = 0;
//var bjCount = 0;
//从服务端的数据库中加载票数
this.getTicketCount = function () {
    var ticketCount = dbProxy.getTicketCount(); //DBProxy.js
    //bjCount = ticketCount[0];
    szCount = ticketCount[1]
}

function loadData(lucky, awardExisted) {
    var data = new Array();
    this.getTicketCount();
    //生成所有抽奖号码
    for (var i = 1; i <= szCount; i++) {
        var paded = "";
        if (i < 10) {
            paded = "000" + i.toString();
        }
        else if (i >= 10 && i < 100) {
            paded = "00" + i.toString();
        }
        else if (i >= 100 && i < 1000) {
            paded = "0" + i.toString();
        }
        else if (i >= 1000) {
            paded = i.toString();
        }
        //paded = "SZ-" + paded;
        // paded =  paded;
        if (lucky.IsCandidate(paded, awardExisted)) data[data.length] = paded; //Lucky.js
    }
    lucky.Init(data, awardExisted);
}
