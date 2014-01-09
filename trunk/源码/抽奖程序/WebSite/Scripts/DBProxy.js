/// <reference path="/Scripts/Lucky.js" />
/// <reference path="/Scripts/jquery-1.7.min.js" />

function DBProxy() {

    this.GetTicket = function () {
        $.ajax({
            type: "POST",
            url: "AwardService.asmx/GetTicket",
            data: "{}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            async: false,
            error: function (jqXHR, textStatus, errorThrown) {
                alert(errorThrown);
            },
            success: function (msg) {
                lucky.Ticket = msg.d.slice();
            }
        });
    }

    this.GetAward = function () {
        var award = null;
        $.ajax({
            type: "POST",
            url: "AwardService.asmx/GetAward",
            data: "{}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            async: false,
            error: function (jqXHR, textStatus, errorThrown) {
                alert(errorThrown);
            },
            success: function (msg) {
                award = msg.d.slice();
            }
        });
        return award;
    }

    this.GetAwardObj = function () {
        var award = null;
        $.ajax({
            type: "POST",
            url: "AwardService.asmx/GetAwardObj",
            data: "{}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            async: false,
            error: function (jqXHR, textStatus, errorThrown) {
                alert(errorThrown);
            },
            success: function (msg) {
                award = msg.d.slice();
            }
        });
        return award;
    }

    this.AddAward = function (ticketNO, level, remark) {
        var jsonData = "{'ticketNO':'" + ticketNO + "','level':'" + level + "','remark':'" + remark + "'}";
        $.ajax({
            type: "POST",
            url: "AwardService.asmx/AddAward",
            data: jsonData,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            async: false,
            error: function (jqXHR, textStatus, errorThrown) {
                alert(errorThrown);
            }
            //            success: function (msg) {
            //                alert(msg.d);
            //            }
        });
    }

    this.AddAwardMany = function (awardMany) {
        //var pDataJson=awardMany.toJSONString();
        var p1 = JSON.stringify({ awards: awardMany });
        $.ajax({
            type: "POST",
            url: "AwardService.asmx/AddAwardMany",
            data: p1,
            //            data: { awards: awardMany },
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            async: false,
            error: function (jqXHR, textStatus, errorThrown) {
                alert(errorThrown);
            }
            //            success: function (msg) {
            //                alert(msg.d);
            //            }
        });
    }

    this.DiscardAward = function (ticketNO) {
        var jsonData = "{'ticketNO':'" + ticketNO + "'}";
        $.ajax({
            type: "POST",
            url: "AwardService.asmx/DiscardAward",
            data: jsonData,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            async: false,
            error: function (jqXHR, textStatus, errorThrown) {
                alert(errorThrown);
            }
            //            success: function (msg) {
            //                alert(msg.d);
            //            }
        });
    }

    this.GenerateAward = function (belongTo) {
        var p1 = "{'belongTo':'" + belongTo + "'}";
        $.ajax({
            type: "POST",
            url: "AwardService.asmx/GenerateAward",
            data: p1,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            async: false,
            error: function (jqXHR, textStatus, errorThrown) {
                alert(errorThrown);
            },
            success: function (msg) {
                //lucky.Award = msg.d.slice();
            }
        });
    }

    ///为了获得票数
    this.getTicketCount = function () {
        var ticketCount = null;
        $.ajax({
            type: "POST",
            url: "AwardService.asmx/GetTicketCount",
            data: "{}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            async: false,
            error: function (jqXHR, textStatus, errorThrown) {
                alert(errorThrown);
            },
            success: function (msg) {
                ticketCount = msg.d.slice();
            }
        });
        return ticketCount;
    }
}

var dbProxy = new DBProxy();