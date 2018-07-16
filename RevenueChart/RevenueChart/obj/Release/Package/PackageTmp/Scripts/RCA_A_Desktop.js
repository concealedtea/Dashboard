$(document).ready(function () {
    var l = document.getElementById("rca_a_desktop"), b = new Chart(l, {
        type: "bar", data: {}, options: {
            fill: !1, scales: {
                xAxes: [{
                    labels: "00:00:00 00:15:00 00:30:00 00:45:00 01:00:00 01:15:00 01:30:00 01:45:00 02:00:00 02:15:00 02:30:00 02:45:00 03:00:00 03:15:00 03:30:00 03:45:00 04:00:00 04:15:00 04:30:00 04:45:00 05:00:00 05:15:00 05:30:00 05:45:00 06:00:00 06:15:00 06:30:00 06:45:00 07:00:00 07:15:00 07:30:00 07:45:00 08:00:00 08:15:00 08:30:00 08:45:00 09:00:00 09:15:00 09:30:00 09:45:00 10:00:00 10:15:00 10:30:00 10:45:00 11:00:00 11:15:00 11:30:00 11:45:00 12:00:00 12:15:00 12:30:00 12:45:00 13:00:00 13:15:00 13:30:00 13:45:00 14:00:00 14:15:00 14:30:00 14:45:00 15:00:00 15:15:00 15:30:00 15:45:00 16:00:00 16:15:00 16:30:00 16:45:00 17:00:00 17:15:00 17:30:00 17:45:00 18:00:00 18:15:00 18:30:00 18:45:00 19:00:00 19:15:00 19:30:00 19:45:00 20:00:00 20:15:00 20:30:00 20:45:00 21:00:00 21:15:00 21:30:00 21:45:00 22:00:00 22:15:00 22:30:00 22:45:00 23:00:00 23:15:00 23:30:00 23:45:00".split(" "),
                    ticks: { autoSkip: !0, maxTicksLimit: 24 }
                }], yAxes: [{ label: [], id: "A", ticks: { beginAtZero: !0 } }]
            }
        }
    }); (function () {
        $.ajax({
            url: "/Revenue/ReceiveClickAllowDesktop", type: "get", dataType: "json", success: function (c) {
                ajaxData = c; b.data.datasets.labels = ajaxData.Date; for (var d = [], e = [], f = [], g = 0, h = 0, k = 0, a = 0; a < c.Date.length; a++) "Today" == ajaxData.DayOfWeek[a] && (d[g] = ajaxData.Allows[a], g++), "Yesterday" == ajaxData.DayOfWeek[a] && (e[h] = ajaxData.Allows[a], h++), "Last Week" == ajaxData.DayOfWeek[a] && (f[k] = ajaxData.Allows[a], k++); b.data.datasets[2] =
                { label: ["Today"], fillColor: "rgb(66, 179, 244)", borderColor: "rgb(66, 179, 244)", borderWidth: 5, data: d, type: "bar" }; b.data.datasets[1] = { label: ["Yesterday"], backgroundColor: "transparent", borderColor: "black", data: e, type: "line" }; b.data.datasets[0] = { label: ["Last Week"], backgroundColor: "transparent", borderColor: "red", data: f, type: "line" }; b.update()
            }, error: function () { console.log("Error") }
        })
    })()
});
