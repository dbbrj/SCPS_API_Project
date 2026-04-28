FROM maven:3.9-eclipse-temurin-21

WORKDIR /Gift_Basket_Production

COPY Gift_Basket_Production/ .

CMD ["bash"]