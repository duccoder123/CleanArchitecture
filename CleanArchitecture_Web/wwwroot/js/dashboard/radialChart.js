function loadRadialBarChart(id, data) {
    var charColors = getChartColorsArray(id);
    var options = {
        fill: {
            colors = charColors;
        },
        chart: {
            height: 120,
            width: 90,
            type: "radialBar",
            sparkline: {
                enable: true
            },
            offsetY: -10,
        },

        series: data.series,

        plotOptions: {
            radialBar: {
                dataLabels: {
                    value: {
                        offsetY: -10,
                    }
                }
            }
        },
        labels: [""],
        stroke: {
            lineCap: "round",
        },
    };
    var chart = new ApexCharts(document.querySelector("#" + id), options);
    chart.render();
}

function getChartColorsArray(id) {
    if (document.getElementById(id) != null) {
        var color = document.getElementById(id).getAttribute("data-colors");
        if (colors) {
            colors = JSON.parse(colors);
            return colors.map(function (value) {
                var newValue = value.replace(" ", "");
                if (newValue.indexOf(",") === -1) {
                    var color = getComputedStyle(document.documentElement).getPropertyValue(newValue);
                    if (color) return color;
                    else return newValue;
                }
            })
        }
    }
}