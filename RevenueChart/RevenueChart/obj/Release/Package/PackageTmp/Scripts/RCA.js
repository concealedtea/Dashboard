$(document).ready(function () {
    var b = document.getElementById("rca_c"), f = new Chart(b, {
        type: "bar", data: {}, options: {
            fill: !1, scales: {
                xAxes: [{
                    labels: "00:00:00 00:15:00 00:30:00 00:45:00 01:00:00 01:15:00 01:30:00 01:45:00 02:00:00 02:15:00 02:30:00 02:45:00 03:00:00 03:15:00 03:30:00 03:45:00 04:00:00 04:15:00 04:30:00 04:45:00 05:00:00 05:15:00 05:30:00 05:45:00 06:00:00 06:15:00 06:30:00 06:45:00 07:00:00 07:15:00 07:30:00 07:45:00 08:00:00 08:15:00 08:30:00 08:45:00 09:00:00 09:15:00 09:30:00 09:45:00 10:00:00 10:15:00 10:30:00 10:45:00 11:00:00 11:15:00 11:30:00 11:45:00 12:00:00 12:15:00 12:30:00 12:45:00 13:00:00 13:15:00 13:30:00 13:45:00 14:00:00 14:15:00 14:30:00 14:45:00 15:00:00 15:15:00 15:30:00 15:45:00 16:00:00 16:15:00 16:30:00 16:45:00 17:00:00 17:15:00 17:30:00 17:45:00 18:00:00 18:15:00 18:30:00 18:45:00 19:00:00 19:15:00 19:30:00 19:45:00 20:00:00 20:15:00 20:30:00 20:45:00 21:00:00 21:15:00 21:30:00 21:45:00 22:00:00 22:15:00 22:30:00 22:45:00 23:00:00 23:15:00 23:30:00 23:45:00".split(" "),
                    ticks: { autoSkip: !1, maxTicksLimit: 96 }
                }], yAxes: [{ label: [], id: "A", ticks: { beginAtZero: !0 } }]
            }
        }
    }); b = document.getElementById("rca_r"); var g = new Chart(b, {
        type: "bar", data: {}, options: {
            fill: !1, scales: {
                xAxes: [{
                    labels: "00:00:00 00:15:00 00:30:00 00:45:00 01:00:00 01:15:00 01:30:00 01:45:00 02:00:00 02:15:00 02:30:00 02:45:00 03:00:00 03:15:00 03:30:00 03:45:00 04:00:00 04:15:00 04:30:00 04:45:00 05:00:00 05:15:00 05:30:00 05:45:00 06:00:00 06:15:00 06:30:00 06:45:00 07:00:00 07:15:00 07:30:00 07:45:00 08:00:00 08:15:00 08:30:00 08:45:00 09:00:00 09:15:00 09:30:00 09:45:00 10:00:00 10:15:00 10:30:00 10:45:00 11:00:00 11:15:00 11:30:00 11:45:00 12:00:00 12:15:00 12:30:00 12:45:00 13:00:00 13:15:00 13:30:00 13:45:00 14:00:00 14:15:00 14:30:00 14:45:00 15:00:00 15:15:00 15:30:00 15:45:00 16:00:00 16:15:00 16:30:00 16:45:00 17:00:00 17:15:00 17:30:00 17:45:00 18:00:00 18:15:00 18:30:00 18:45:00 19:00:00 19:15:00 19:30:00 19:45:00 20:00:00 20:15:00 20:30:00 20:45:00 21:00:00 21:15:00 21:30:00 21:45:00 22:00:00 22:15:00 22:30:00 22:45:00 23:00:00 23:15:00 23:30:00 23:45:00".split(" "),
                    ticks: { autoSkip: !1, maxTicksLimit: 96 }
                }], yAxes: [{ label: [], id: "A", ticks: { beginAtZero: !0 } }]
            }
        }
    }); (function () {
        $.ajax({
            url: "/Revenue/ReceiveClickAllow", type: "get", dataType: "json", success: function (b) {
                ajaxData = b; for (var h = [], k = [], l = [], c, d, e, a = e = d = c = 0; a < b.Date.length; a++) "Today" == ajaxData.DayOfWeek[a] && (h[c] = ajaxData.Clicks[a], c++), "Yesterday" == ajaxData.DayOfWeek[a] && (k[d] = ajaxData.Clicks[a], d++), "Last Week" == ajaxData.DayOfWeek[a] && (l[e] = ajaxData.Clicks[a], e++); f.data.datasets[2] = {
                    label: ["Today"], fillColor: "rgb(66, 179, 244)",
                    borderColor: "rgb(66, 179, 244)", borderWidth: 5, data: h, type: "bar"
                }; f.data.datasets[1] = { label: ["Yesterday"], backgroundColor: "transparent", borderColor: "black", data: k, type: "line" }; f.data.datasets[0] = { label: ["Last Week"], backgroundColor: "transparent", borderColor: "red", data: l, type: "line" }; f.update(); for (a = e = d = c = 0; a < b.Date.length; a++) "Today" == ajaxData.DayOfWeek[a] && (h[c] = ajaxData.Receives[a], c++), "Yesterday" == ajaxData.DayOfWeek[a] && (k[d] = ajaxData.Receives[a], d++), "Last Week" == ajaxData.DayOfWeek[a] && (l[e] =
                ajaxData.Receives[a], e++); g.data.datasets[2] = { label: ["Today"], fillColor: "rgb(66, 179, 244)", borderColor: "rgb(66, 179, 244)", borderWidth: 5, data: h, type: "bar" }; g.data.datasets[1] = { label: ["Yesterday"], backgroundColor: "transparent", borderColor: "black", data: k, type: "line" }; g.data.datasets[0] = { label: ["Last Week"], backgroundColor: "transparent", borderColor: "red", data: l, type: "line" }; g.update()
            }, error: function () { console.log("Error") }
        })
    })()
});
