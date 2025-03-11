export default {
    async fetch(request, env) {
        const url = new URL(request.url);
        const params = url.searchParams;


        const distanceToTarget = parseFloat(params.get('distanceToTarget'));


        const rainImpact = parseFloat(params.get('rain')); // Light rain
        const windSpeed = parseFloat(params.get('windSpeed')); // Wind speed in km/h
        const windDirection = params.get('windDirection') || 'against'; // Wind direction
        const humidity = parseFloat(params.get('humidity')); // Wind speed in km/h
        const pressure = parseFloat(params.get('pressure')); // Wind speed in km/h

        console.log(windSpeed);
        const clubsParam = params.get('clubs') || 'Driver:250,3 Hybrid:230';
        const clubs = clubsParam.split(',').map(club => {
            const [name, distance] = club.split(':');
            return { name: name.trim(), distance: parseFloat(distance) };
        });


        const clubDescriptions = clubs
            .map(club => `${club.name} ${club.distance} yards`)
            .join(' and my ');


        const prompt = `
      I hit my ${clubDescriptions}.
      If there is ${rainImpact}mm of rain, a ${humidity}% humidity, a ${windSpeed} km/h wind ${windDirection}, a ${pressure} pressure (where the standard is 1013.3 milli bars)
      and the target is ${distanceToTarget} yards away, which club should I use?
      Please consider how each of the weather factors will impact the distance a ball will travel; assume the wind factor will reduce the distance of the ball.
      Then compare this effect to the stock yardages for this user's clubs.
      Only recommend a club from the options I have available (This is very important).
      Please respond with the following: repeat the inputted distance and parameters, say what club to use and how hard as a percent to hit it, do not say anything else.
    `;


        // Call the AI model with the prompt
        const input = {
            prompt: prompt
        };
        const response = await env.AI.run('@cf/meta/llama-3-8b-instruct', input);


        // Structure response
        const task = {
            inputs: {
                distanceToTarget,
                rainImpact,
                windSpeed,
                windDirection,
                clubs
            },
            response
        };
        return new Response(JSON.stringify(task), { headers: { 'Access-Control-Allow-Origin': '*' } });


    }
};
