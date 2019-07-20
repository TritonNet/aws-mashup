api_root = "https://laabe461ak.execute-api.eu-central-1.amazonaws.com/Production"
//api_root = "https://0k3vhfpqi7.execute-api.eu-central-1.amazonaws.com/production";
//api_root = "http://localhost:57924"

charts = []
all_products = []

$(document).ready(function () {

    $('.cumulus-list-group').on('click', 'li', function () { loadFullChart($(this).attr("data-productindex")); });

    $(window).on('resize', function () {
        for (var index = 0; index < charts.length; index++) {
            charts[index].setSize($(".trending-chart-container").width() / 3);
        }
    });

    loadProducts();
    loadTrendingCharts();
});

function loadTrendingCharts() {

    var chartWidth = $(".trending-chart-container").width() / 3;

    for (var index = 0; index < 6; index++) {
        chartOption = {
            series: [{
                data: []
            }]
        }

        var chart_container_id = 'trending-chart-container-' + index;

        var chart = createChart(chart_container_id, 'trending', null, chartOption);
        chart.showLoading()
        chart.setSize(chartWidth);

        charts[index] = chart;
    }

    $.getJSON(api_root + "/api/products/trending", function (productCollection) {

        $.each(productCollection, function (index, product) {
            api_host = api_root + "/api/products/" + product.productISIN + "/trades_num";

            $.getJSON(api_host, function (data) {

                var chart = charts[index];

                chart.hideLoading();
                
                chart.setTitle({ text: product.productName }, { text: product.stockMarket == 1 ? "Eurex Stock Market" : "Xetra Stock Market" });

                chart.series[0].setData(data);
            });
        });
    });
}

function loadFullChart(product_index) {

    product = all_products[product_index]

    $('.trending-chart-container').hide('fast');

    $("html, body").animate({ scrollTop: 0 }, "slow");

    const names = [['min', 'Minimum Price'], ['max', 'Maximum Price'], ['end', 'End Price'], ['start', 'Start Price'], ['predicted', 'Predicted End Price']];
    var seriesOptions = [];

    var fullChart = createChart('full-chart-container', 'full', product, seriesOptions);
    fullChart.showLoading()

    seriesCounter = 0;

    $.each(names, function (index, name) {

        api_host = api_root + "/api/products/" + product['productISIN'] + "/trades_";
        $.getJSON(api_host + name[0], function (data) {

            fullChart.addSeries({
                name: name[1],
                data: data
            }, false);

            // As we're loading the data asynchronously, we don't know what order it will arrive. So
            // we keep a counter and create the chart when all the data is loaded.
            seriesCounter += 1;

            if (seriesCounter === names.length) {
                fullChart.hideLoading();
                fullChart.redraw();
                fullChart.setSize($(".trending-chart-container").width());
            }
        });
    });
}


function loadProducts() {
    api_host = api_root + "/api/products/all"
    $.getJSON(api_host + name, function (data) {

        all_products = data;

        $.each(data, function (product_idx) {
            product = data[product_idx]

            li = '<li class="list-group-item cumulus-list-group-item" data-productindex=' + product_idx + '>' + product['productName'] + '</li>'

            $("#product-list").append(li).show('slow');;
        });
    });
}

function loadProduct() {
    alert($(this).attr("data-productisin"));
}

function loadChart(product) {


}

function createChart(container_id, chart_type, product, seriesOptions) {

    return Highcharts.stockChart(container_id, {

        rangeSelector: {
            selected: 4,
            inputEnabled: (chart_type == 'full'),
            buttonTheme: {
                visibility: (chart_type == 'full') ? 'visible' : 'hidden'
            },
            labelStyle: {
                visibility: (chart_type == 'full') ? 'visible' : 'hidden'
            }
        },

        yAxis: {
            labels: {
                formatter: function () {
                    return (this.value > 0 ? ' + ' : '') + this.value + '%';
                }
            },
            plotLines: [{
                value: 0,
                width: 2,
                color: 'silver'
            }]
        },

        plotOptions: {
            series: {
                compare: 'percent',
                showInNavigator: true
            }
        },

        tooltip: {
            pointFormat: '<span style="color:{series.color}">{series.name}</span>: <b>{point.y}</b> ({point.change}%)<br/>',
            valueDecimals: 2,
            split: true
        },

        series: seriesOptions.series,

        legend: { enabled: chart_type == 'full' },

        title: {
            text: (product != null ? product.productName : '')
        },
        subtitle: {
            text: (product != null ? (product.stockMarket == 1 ? "Eurex Stock Market" : "Xetra Stock Market") : '')
        },

        scrollbar: {
            enabled: chart_type == 'full'
        },

        responsive: {
            rules: [{
                condition: {
                    maxWidth: 500
                },
                chartOptions: {
                    chart: {
                        height: chart_type == 'full' ? 500 : 300
                    },
                    subtitle: {
                        text: null
                    },
                    navigator: {
                        enabled: false
                    }
                }
            }]
        }
    });
}