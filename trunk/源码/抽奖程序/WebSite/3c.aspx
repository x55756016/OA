<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>三等奖抽奖（第三轮）</title>
    <script type="text/javascript" src="Scripts/jquery-1.7.min.js"></script>
    <script type="text/javascript" src="Scripts/json2.js"></script>
    <script type="text/javascript" src="Scripts/Lucky.js"></script>
    <script type="text/javascript" src="Scripts/DBProxy.js"></script>
    <script type="text/javascript" src="default.js"></script>
   <style>
        body, html
        {
            padding: 0;
            margin: 0;
            height: 100%;
            overflow: hidden;
        }
        body
        {            
            font-size: 1em;
            font-family: Tahoma, Geneva, sans-serif;
            background-color: #000;
            color: #fff;
            font-weight: bold;
           
        }
        ul,li{ margin:0; padding:0;}
       
        .list_num
        { 
            width: 520px;
            margin: 0;
            padding: 0;
            text-align: center;
            overflow: hidden;
        }
        .list_num li
        {
            list-style: none;
            padding: 0 0 5px 0;
            margin: 0;
            font-weight: bold;            
            white-space: nowrap;
            word-break: keep-all;
            border-bottom:1px solid rgba(177,96,1,0.5);
            border-top:1px solid rgba(255,255,255,0.5);
            color: #a20000;
        }
        .list_num li:first-child { border-top:0;}
        .list_num li:last-child { border-bottom:0;}
        #num_award
        {
            display: none;
            text-shadow: 0px 2px 1px rgba(255,255,255,0.5);
        }
        .sz_award,.bj_award{  position:absolute;   list-style-type:none; padding:10px 0;  /*background-image: -webkit-linear-gradient(top, rgba(0,0,0,0.5), rgba(0,0,0,0.1), rgba(0,0,0,0.5))*/;}
       .sz_award li,.bj_award li{  float:left;color:#FFF1B4; text-align:center; width:49%;border-radius: 14px; margin:0 0 4px 2px; padding:2px 0;
                                    text-shadow: 0px 1px 2px #000; font-weight:bold; box-shadow: 0px 2px 4px #000; border-top:1px solid rgba(252,252,162,0.4);
                                  background:#a60000; background: -webkit-gradient(linear, left top, left bottom, from(#c40e00), to(#a60000));}
        #roll_num,#num_award{  margin: auto; position: absolute;z-index: 9999; left:50%;}
        .num_mask_top{ background:url(images/num_mask_top.png) no-repeat; width:571px; height:20px; position:absolute; top:203px; left:50%; margin-left:-285px; z-index:9999;}
        .num_mask_bottom{ background:url(images/num_mask_bottom.png) no-repeat; width:571px; height:20px; position:absolute; top:618px; left:50%; margin-left:-285px; z-index:9999;}
         .stage_left {
-webkit-animation: sl 2s 1 linear;
}
@-webkit-keyframes sl {
from { -webkit-transform:  skew(0deg) ;  }
to { -webkit-transform:  skew(-50deg) ; }
}

.stage_right {
-webkit-animation: sr 2s 1 linear;
}
@-webkit-keyframes sr {
from { -webkit-transform:  skew(0deg) ;  }
to { -webkit-transform: skew(40deg) ; }
}
    </style>
    <script type="text/javascript">
        $(function () {
            all_sn();
        })
        lucky.PublishBatch = "Batch3";
        var playing = true;
        var cur_lev = 3;
        var cur_num = 5;
        var open = false;
        var batch = "Batch3";  //当前奖项和奖票数目
        $(function () {

        })
        document.onkeyup = function (e) {
            var theEvent = window.event || e;
            var code = theEvent.keyCode || theEvent.which;
            //alert(event.keyCode);
            if (code == 32) {//按空格键
                if (!open) {
                    open = true;
                    stage();

                } else {
                    if (playing) {
                        stop(cur_lev, cur_num); //停止动画，显示获奖号
                        playing = false;
                    } else {
                        start(); //开始
                        playing = true;
                    }
                }
                return false
            }
            if (code == 46 && !playing) {//按删除键
                var $children_li = $("#num_award").children();
                $children_li.each(function () {
                    var del_award = $(this).text();
                    lucky.DiscardAward(del_award); //获取当前选号，选票删除
                    dbProxy.DiscardAward(del_award); //获取当前选号，并传服务器
                })
                show_award(cur_lev, batch);
                start(); //开始
                playing = true;
                return false
            }
        }


        window.onresize = function () { resize(cur_lev) };
        window.onload = function () { resize(cur_lev) };
    </script>
</head>
<body>
<img src="images/page_back_award3.jpg" style="width:100%; position:absolute; z-index:-1;" id="back_img" />
    <ul id="roll_num" class="list_num"></ul>
    <ul id="num_award" class="list_num"></ul>
    <ul class="sz_award">
    </ul>
    <ul class="bj_award">
    </ul>
    <div id="stage" style="width: 100%; height:100%; position: absolute; z-index: 9999;">  
        <div style="position:absolute;left:0;top:0;width:100%;height:100%;background-color:#000" id="black_mask"></div>      
        <img src="images/stage_left02.jpg" width="50%" id="stage_left02" style=" position:absolute; left:0;top:0;" /> 
        <img src="images/stage_right02.jpg" width="50%" id="stage_right02" style=" position:absolute; right:0;top:0;"/>
        <img src="images/stage_left01.png" width="16.85%" id="stage_left01" style=" position:absolute; left:0; top:0;" /> 
        <img src="images/stage_right01.png" width="16.85%" id="stage_right01" style=" position:absolute; right:0;top:0;"/>
        <img src="images/stage_top.png" width="100%" id="stage_top" style=" position:absolute; top:0;"/>
    </div>

</body>
</html>
