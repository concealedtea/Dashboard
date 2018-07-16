$(document).ready(function () {
    var d = document.getElementById("revenue"), a = new Chart(d, { type: "bar", data: {}, options: { autoSkip: !1, responsive: !0, maintainAspectRatio: !1, scales: { xAxes: [{ label: "date", stacked: !0, ticks: { maxTicksLimit: 60 } }], yAxes: [{ label: "revenue", stacked: !0, position: "left", id: "A", ticks: { beginAtZero: !0, stepSize: 1E3 } }] } } }); (function () {
        $.ajax({
            url: "/Revenue/TotalRev", type: "get", dataType: "json", success: function (c) {
                ajaxData = c; a.data.labels = ajaxData[2].Date; for (var b = 0; b < c.length; b++) a.data.datasets[b] =
                { label: ajaxData[b].Advertiser, backgroundColor: "rgb(" + Math.floor(256 * Math.random()) + "," + Math.floor(256 * Math.random()) + "," + Math.floor(256 * Math.random()) + ")", data: ajaxData[b].Revenue, yAxisID: "A" }; a.update()
            }, error: function () { console.log("Error") }
        })
    })(); window.matchMedia("@media (min-width: 600px)").matches ? a.options.legend.display = !0 : a.options.legend.display = !1; a.update()
});
