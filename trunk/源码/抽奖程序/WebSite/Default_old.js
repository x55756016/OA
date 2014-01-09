//加载数据
function creat_rd_arry() {
    if (lucky.Ticket.length == 0) {
        var awardExisted = dbProxy.GetAwardObj();
        loadData(lucky, awardExisted);
    }
}

//开始滚动
function stop(level, number) {
    //获取得奖号
    var obj = lucky.GenerateAward(level, number);
    if (cur_lev > 0) {//除特等奖外，显示获奖号码。
        show_award(cur_lev);
    }
    //插入获奖号
    var award_li_html = ''
    for (i = 0; i < number; i++) {
        award_li_html += "<li>" + obj[i].TicketNO + "</li>";
    }
    $("#num_award")[0].innerHTML = award_li_html;

    //停止动画，显示得奖号
    $("#num_award").css("display", "block");
    $("#roll_num").css("display", "none");

    //上传获奖号
    dbProxy.AddAwardMany(obj);
    $("#yh").show();
}
function start() {
    $("#yh").hide();
    $("#roll_num").css("display", "block");
    $("#num_award").empty().css("display", "none");
}

//开场动画
function stage() {
    creat_rd_arry();
    $("#stage_left").addClass("stage_left").animate({ left: -1040 }, 2000)
    $("#stage_right").addClass("stage_right").animate({ left: 1580 }, 2000)
    $("#black_stage").fadeOut(2000, function () { $("#stage").remove(); });
}
//两边显示获奖号

function show_award(cur_lev) {
    var awards_sz = lucky.GetAwardBy("SZ", cur_lev);
    var awards_bj = lucky.GetAwardBy("BJ", cur_lev);
    var sz_li = '', bj_li = '';
    for (i = awards_sz.length - 1; i >= 0; i--) {
        if (awards_sz[i] != undefined) {
            sz_li += "<li>" + awards_sz[i] + "</li>";
        }
    }
    for (i = awards_bj.length - 1; i >=0; i--) {
        if (awards_bj[i] != undefined) {
            bj_li += "<li>" + awards_bj[i] + "</li>";
        }
    }

    $(".sz_award")[0].innerHTML = sz_li;
    $(".bj_award")[0].innerHTML = bj_li;

}

function stop_loc(loc) {
    //获取得奖号
    if (loc == 'BJ') {
        var obj = lucky.GenerateAward0BJ();
    } else {
        var obj = lucky.GenerateAward0SZ();
    }
    //插入获奖号    
    $("#num_award").append("<li>" + obj.TicketNO + "</li>");

    //停止动画，显示得奖号
    $("#num_award").css("display", "block");
    $("#roll_num").css("display", "none");

    //上传获奖号
    dbProxy.AddAward(obj.TicketNO, obj.Level);
    $("#yh").show();
}